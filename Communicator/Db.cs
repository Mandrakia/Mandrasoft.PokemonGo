using EntityFramework.BulkInsert.Extensions;
using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.Logging;
using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models;
using MandraSoft.PokemonGo.Models.PocoProtos;
using POGOProtos.Map;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Communicator
{
    public static class Db
    {
        static public SemaphoreSlim DbUpdateSemaphore = new SemaphoreSlim(1, 1);
        static private IEnumerable<WildPokemonPoco> _Backup;
        static private SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);
        static private DateTime _lastSentData = DateTime.MinValue;
        static private Dictionary<ulong, WildPokemonPoco> _Buffer = new Dictionary<ulong, WildPokemonPoco>();
        static private object _BufferLock = new object();
        /// <summary>
        /// WebSite Callback meant te be passed to PokemonGoClient.MapObjectsHandler
        /// Can be called by multiple threads and multiple clients.
        /// Will only send to webSite every 1 min.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        static public async Task UpdateResponseToDb(PokemonGoClient client, GetMapObjectsResponse resp)
        {
            // Making sure it's a success before going on with the treatment.
            if (resp.Status != MapObjectsStatus.Success) return;
            // Converting POGProto to Poco ObjectList
            // Because when i tried to keep the state as POGProtos I kept having exception when adding/removing from RepeatedFields
            // Anyway all the serialization overhead was completely useless and a resource sink.
            var nCellList = new List<MapCellPoco>();
            foreach (var cell in resp.MapCells)
                nCellList.Add(new MapCellPoco(cell));
            foreach (var wild in nCellList.SelectMany(x => x.WildPokemons))
            {
                lock (_BufferLock)
                {
                    if (!_Buffer.ContainsKey(wild.EncounterId))
                        _Buffer.Add(wild.EncounterId, wild);
                    else
                        _Buffer[wild.EncounterId] = wild;
                }
            }
            if ((DateTime.UtcNow - _lastSentData).TotalSeconds > Configuration.DbDelay)
            {
                if (await _Semaphore.WaitAsync(1))
                {
                    List<WildPokemonPoco> listToSend = new List<WildPokemonPoco>();
                    try
                    {
                        lock (_BufferLock)
                        {
                            listToSend = _Buffer.Values.ToList();
                            _Buffer = new Dictionary<ulong, WildPokemonPoco>();
                        }
                        if (_Backup.Any())
                        {
                            listToSend.AddRange(_Backup);
                        }
                        await UpdateResponseToDb(listToSend);
                        _lastSentData = DateTime.UtcNow;
                    }
                    catch (Exception e)
                    {
                        _Backup = listToSend;
                        Logger.Write("Error sending values to the website");
                        Logger.Write(e.Message);
                    }
                    finally
                    {
                        _Semaphore.Release();
                    }
                }
            }
        }
        static public async Task UpdateResponseToDb(IEnumerable<WildPokemonPoco> pokemons)
        {
            //Transform WildPokemonPoco to List of encounters/SpawnPoints.
            var spawnPoints = new Dictionary<string, Models.Entities.SpawnPoint>();
            var encounters = new Dictionary<ulong, Models.Entities.Encounter>();
            foreach (var wld in pokemons)
            {
                if (!spawnPoints.ContainsKey(wld.SpawnPointId))
                    spawnPoints.Add(wld.SpawnPointId, new Models.Entities.SpawnPoint() { Id = wld.SpawnPointId, Location = DbGeography.FromText("POINT(" + wld.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + wld.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")"), Encounters = new List<Models.Entities.Encounter>() });
                if (!encounters.ContainsKey(wld.EncounterId))
                    encounters.Add(wld.EncounterId, new Models.Entities.Encounter() { IdU = wld.EncounterId, PokemonId = (int)wld.PokemonData.PokemonId, SpawnPointId = wld.SpawnPointId, EstimatedSpawnTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(wld.LastModifiedTimestampMs).AddMilliseconds(wld.TimeTillHiddenMs).AddMinutes(-15), ExpirationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(wld.LastModifiedTimestampMs).AddMilliseconds(wld.TimeTillHiddenMs) });

            }
            if (await DbUpdateSemaphore.WaitAsync(-1))
            {
                try
                {
                    using (var ctx = new PokemonDb())
                    {

                        var currentIdsSpawns = await ctx.SpawnPoints.Select(x => x.Id).ToListAsync();
                        var currentIdsEncounters = await ctx.Encounters.Select(x => x.Id).ToListAsync();
                        foreach (var idSpawn in currentIdsSpawns)
                            if (spawnPoints.ContainsKey(idSpawn)) spawnPoints.Remove(idSpawn);
                        foreach (var idEncount in currentIdsEncounters)
                        {
                            ulong nId;
                            unchecked
                            {
                                nId = (ulong)idEncount;
                            }
                            if (encounters.ContainsKey(nId)) encounters.Remove(nId);
                        }
                        if (spawnPoints.Any())
                            ctx.BulkInsert(spawnPoints.Values.ToList());
                        if (encounters.Any())
                            ctx.BulkInsert(encounters.Values.ToList());
                    }
                }
                finally
                {
                    DbUpdateSemaphore.Release();
                }
            }
        }
    }
}

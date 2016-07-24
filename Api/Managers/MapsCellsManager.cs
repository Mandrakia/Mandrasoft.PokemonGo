using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models.PocoProtos;
using MandraSoft.PokemonGo.Api.Extensions;
using MandraSoft.PokemonGo.Api.Helpers;
using POGOProtos.Map;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Extensions.Compression.Client;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Managers
{
    public class MapsCellsManager : IDisposable
    {
        public ConcurrentDictionary<ulong, MapCellPoco> MapsCells { get; set; }
        public IEnumerable<FortDataPoco> AllPokestops => MapsCells.Values.SelectMany(x => x.Forts.Where(a => a.Type == FortType.Checkpoint));
        public IEnumerable<FortDataPoco> AvailablePokestops => AllPokestops.Where(x => x.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime());

        public IEnumerable<MapCellPoco> GetNearbyCells(double lat, double lng)
        {

            var nearbyCellIds = S2Helper.GetNearbyCellIds(lat, lng);
            return MapsCells.Where(x => nearbyCellIds.Contains(x.Key)).Select(x => x.Value);


        }
        internal List<long> GetLastUpdatedTimestamp(IList<ulong> CellIds)
        {
            //return new long[CellIds.Count].ToList();
            var result = new List<long>();
            foreach (var cellId in CellIds)
            {
                if (MapsCells.ContainsKey(cellId))
                    result.Add(MapsCells[cellId].CurrentTimestampMs);
                else
                    result.Add(0);
            }
            return result;
        }

        public MapsCellsManager()
        {
            MapsCells = new ConcurrentDictionary<ulong, MapCellPoco>();
        }

        private SemaphoreSlim _semaphorePurge = new SemaphoreSlim(1, 1);

        private DateTime _lastPurge = DateTime.MinValue;
        /// <summary>
        /// Called by the GenericResponseHandler each time a response of type GetMapObjectsResponse is received.
        /// </summary>
        /// <param name="resp"></param>
        internal void UpdateMapCells(GetMapObjectsResponse resp)
        {
            // Clearing expired Pokemons
            // Only 1 thread need to do it hence the _semaphore, not blocking if a thread is already doing it
            // the others will just go on.
            if ((DateTime.UtcNow - _lastPurge).TotalSeconds > 5)
            {
                if (_semaphorePurge.Wait(1))
                {
                    try
                    {

                        foreach (var cell in MapsCells.Values)
                        {
                            var toBePurged2 = cell.WildPokemons.Where(x => DateTime.UtcNow > new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(x.LastModifiedTimestampMs).AddMilliseconds(x.TimeTillHiddenMs)).ToList();
                            toBePurged2.ForEach(x => cell.WildPokemons.Remove(x));
                            var toBePurged = cell.CatchablePokemons.Where(x => x.ExpirationTimestampMs < DateTime.UtcNow.ToUnixTime()).ToList();
                            toBePurged.ForEach(x => cell.CatchablePokemons.Remove(x));
                        }
                        _lastPurge = DateTime.UtcNow;
                    }
                    finally
                    {
                        _semaphorePurge.Release();
                    }
                }
            }
            // Making sure it's a success before going on with the treatment.
            if (resp.Status != MapObjectsStatus.Success) return;
            // Converting POGProto to Poco ObjectList
            // Because when i tried to keep the state as POGProtos I kept having exception when adding/removing from RepeatedFields
            // Anyway all the serialization overhead was completely useless and a resource sink.
            var nCellList = new List<MapCellPoco>();
            foreach (var cell in resp.MapCells)
                nCellList.Add(new MapCellPoco(cell));
            // Adding/Merging new cells Info.
            foreach (var cell in nCellList)
            {
                MapsCells.AddOrUpdate(cell.S2CellId, cell, (id, oVal) => MergeMapCell(id, oVal, cell));
            }
        }
        /// <summary>
        /// Handle the merging between Info about a cell and oldInfo.
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="oldCell"></param>
        /// <param name="newCell"></param>
        /// <returns></returns>
        private MapCellPoco MergeMapCell(ulong cellId, MapCellPoco oldCell, MapCellPoco newCell)
        {
            //locking the oldCell so that 2 or more thread can't update it at the same time
            //blocking lock.
            lock (oldCell)
            {
                oldCell.CurrentTimestampMs = newCell.CurrentTimestampMs;
                oldCell.DecimatedSpawnPoints = newCell.DecimatedSpawnPoints; //Always sent replacing old State.
                oldCell.SpawnPoints = newCell.SpawnPoints; //Always send replacing old State.

                //TODO: Handle FortSummaries.


                foreach (var fort in newCell.Forts)
                {
                    if (oldCell.Forts.Any(a => a.Id == fort.Id))
                    {
                        var oFort = oldCell.Forts.SingleOrDefault(x => x.Id == fort.Id);
                        if (oFort.LastModifiedTimestampMs < fort.LastModifiedTimestampMs)
                            oldCell.Forts[oldCell.Forts.IndexOf(oFort)] = fort;
                    }
                    else oldCell.Forts.Add(fort);
                }
                foreach (var wild in newCell.WildPokemons)
                {
                    if (!oldCell.WildPokemons.Any(a => a.EncounterId == wild.EncounterId))
                    {
                        oldCell.WildPokemons.Add(wild);
                    }
                    else
                    {
                        var oWild = oldCell.WildPokemons.SingleOrDefault(x => x.EncounterId == wild.EncounterId);
                        if (oWild.LastModifiedTimestampMs < wild.LastModifiedTimestampMs)
                            oldCell.WildPokemons[oldCell.WildPokemons.IndexOf(oWild)] = wild;
                    }
                }
                //Catchable Pokemons shouldn't be a DeltaResponse it's always a present state of what you can catch in that cell if you are in Catching Radius.
                //In our API it's pretty ambiguous what to do with it.
                //If multithreaded it's just a useless Information because our location changes all the time.
                //I'll just keep adding new entries and purging expired ones.
                foreach (var catchable in newCell.CatchablePokemons)
                {
                    if (!oldCell.CatchablePokemons.Any(a => a.EncounterId == catchable.EncounterId))
                    {
                        oldCell.CatchablePokemons.Add(catchable);
                    }
                }
                return oldCell;
            }
        }
        public void Dispose()
        {
            if (_semaphorePurge != null)
            {
                _semaphorePurge.Dispose();
                _semaphorePurge = null;
            }
        }
    }
}

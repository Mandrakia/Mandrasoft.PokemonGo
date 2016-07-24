using Google.Common.Geometry;
using GoogleMapsApi.Entities.Common;
using Hangfire;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MandraSoft.PokemonGo.Models.WebModels.Mixed;
using MandraSoft.PokemonGo.Models.PocoProtos;
using MandraSoft.PokemonGo.DAL;
using EntityFramework.BulkInsert.Extensions;
using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.ClientExtensions;

namespace MandraSoft.PokemonGo.Web.Jobs
{
    public static class ScanningJobs
    {
        static internal List<SpawnPoint> _Backup;
        static public ConcurrentDictionary<string, Location> Locs = new ConcurrentDictionary<string, Location>();

        static public async Task DumpDataToDatabase()
        {
            List<SpawnPoint> tmpList;
            lock (Globals.DumpQueueLock)
            {
                tmpList = Globals.DumpQueue.ToList();
                Globals.DumpQueue = new List<SpawnPoint>();
            }
            if (_Backup != null)
            {
                tmpList.AddRange(_Backup);
                _Backup = null;
            }

            //Should be safe now only thread accessing the objects.
            var spawnPoints = new Dictionary<string, MandraSoft.PokemonGo.Models.Entities.SpawnPoint>();
            var encounters = new Dictionary<ulong, MandraSoft.PokemonGo.Models.Entities.Encounter>();
            foreach (var sp in tmpList)
            {
                if (!spawnPoints.ContainsKey(sp.Id))
                    spawnPoints.Add(sp.Id, new MandraSoft.PokemonGo.Models.Entities.SpawnPoint() { Id = sp.Id, Location = DbGeography.FromText("POINT(" + sp.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + sp.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")"), Encounters = new List<MandraSoft.PokemonGo.Models.Entities.Encounter>() });
                foreach (var ec in sp.Encounters)
                {
                    if (!encounters.ContainsKey(ec.Id))
                        encounters.Add(ec.Id, new MandraSoft.PokemonGo.Models.Entities.Encounter() { IdU = ec.Id, PokemonId = ec.PokemonId, SpawnPointId = sp.Id, EstimatedSpawnTime = ec.SpawnTime, ExpirationTime = ec.SpawnTime.AddMinutes(15) });
                }
            }

            if (await Communicator.Db.DbUpdateSemaphore.WaitAsync(-1))
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
                        try
                        {
                            if (spawnPoints.Any())
                                ctx.BulkInsert(spawnPoints.Values.ToList());
                            if (encounters.Any())
                                ctx.BulkInsert(encounters.Values.ToList());
                        }
                        catch (Exception e)
                        {
                            _Backup = tmpList;
                            throw;
                        }
                    }
                }
                finally
                {
                    Communicator.Db.DbUpdateSemaphore.Release();
                }
            }
        }
        static public async Task ScanAllArea(double lat1, double lng1, double lat2, double lng2, int splittedIn, int indexNumber, string clientCacheName)
        {
            var client = (PokemonGoClient)HttpRuntime.Cache[clientCacheName];
            var region_rect = S2LatLngRect.FromPointPair(
                S2LatLng.FromDegrees(lat1, lng1),
                S2LatLng.FromDegrees(lat2, lng2));
            var coverer = new S2RegionCoverer() { MaxLevel = 15, MinLevel = 15, LevelMod = 0, MaxCells = int.MaxValue };
            var covering = new List<S2CellId>();
            coverer.GetCovering(region_rect, covering);
            covering = covering.OrderBy(x => x.Id).ToList();
            covering = covering.Skip((covering.Count / splittedIn) * indexNumber).Take(covering.Count / splittedIn).ToList();
            foreach (var cell in covering)
            {
                for (S2CellId c = cell.ChildBegin; c != cell.ChildEnd; c = c.Next)
                {
                    var point2 = new S2LatLng(new S2Cell(c).Center);
                    client.SetCoordinates(point2.LatDegrees, point2.LngDegrees);
                    await client.UpdateMapObjects();
                }
            }
        }


        [AutomaticRetry(Attempts = 0)]
        static public async Task ScanAllPokemonsForAddress(string address, string clientCacheName)
        {
            if (!Locs.ContainsKey(address))
            {
                var locationService = GoogleMapsApi.GoogleMaps.Geocode.Query(new GoogleMapsApi.Entities.Geocoding.Request.GeocodingRequest() { ApiKey = "AIzaSyAnQjZ6_1U2aucQsYxr5cdFOcjbNd3S_x4", Address = address });
                var point = locationService.Results.First().Geometry.Location;
                Locs.AddOrUpdate(address, point, (a, b) => point);
            }
            await ScanAllPokemons(Locs[address].Latitude, Locs[address].Longitude, clientCacheName);
        }
        [AutomaticRetry(Attempts = 0)]
        public static async Task ScanAllPokemons(double lat, double lng, string clientCacheName)
        {
            var client = (PokemonGoClient)HttpRuntime.Cache[clientCacheName];
            client.SetCoordinates(lat, lng);
            await client.UpdateMapObjects();
            var pos = 1;
            var x = 0f;
            var y = 0f;
            var dx = 0f;
            var dy = -1f;
            var nLat = lat;
            var nLng = lng;
            for (var steplimit = 1; steplimit < 45; steplimit++)
            {
                var parent = S2CellId.FromLatLng(S2LatLng.FromDegrees(nLat, nLng)).ParentForLevel(15);
                for (S2CellId c = parent.ChildBegin; c != parent.ChildEnd; c = c.Next)
                {
                    var point = new S2LatLng(new S2Cell(c).Center);
                    client.SetCoordinates(point.LatDegrees, point.LngDegrees);
                    await client.UpdateMapObjects();
                }
                if (-steplimit / 2 < x && x <= steplimit / 2 && -steplimit / 2 < y && y <= steplimit / 2)
                {
                    nLat = (x * 0.0025) + lat;
                    nLng = (y * 0.0025) + lng;
                }
                if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
                {
                    var oDx = dx;
                    dx = -dy;
                    dy = oDx;
                }
                x += dx;
                y += dy;

            }
        }
    }
}
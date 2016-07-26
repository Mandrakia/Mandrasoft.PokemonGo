﻿using Google.Common.Geometry;
using MandraSoft.PokemonGo.Api.Helpers;
using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models.WebModels.Mixed;
using MandraSoft.PokemonGo.Models.WebModels.Requests;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web.Controllers
{
    public class SpawnPointsController : ApiController
    {
        [HttpGet]
        public async Task<List<SpawnPoint>> GetAllSpawnPointsForBounds(MapQuery query)
        {
            using (var ctx = new PokemonDb())
            {
                DbGeography box = DbGeography.PolygonFromText(
                string.Format(System.Globalization.CultureInfo.InvariantCulture, "POLYGON(({0} {1},{2} {1},{2} {3},{0} {3},{0} {1}))",
                             query.swLng,
                             query.swLat,
                             query.neLng,
                             query.neLat
                    ), 4326);
                var sp = await ctx.SpawnPoints.Include("Encounters").Where(x => x.Location.Intersects(box)).ToListAsync();
                var spR = sp.Select(x => new SpawnPoint() { Id = x.Id, Latitude = x.Location.Latitude.Value, Longitude = x.Location.Longitude.Value, Encounters = x.Encounters.OrderBy(b => b.EstimatedSpawnTime).Select(a => new Encounter() { PokemonId = a.PokemonId, PokemonName = Globals.PokemonNamesById[a.PokemonId][query.lang], SpawnTime = a.EstimatedSpawnTime }).ToList() }).ToList();
                return spR;
            }
        }
        [HttpPost]
        [ActionName("GetAllPokemonSpawns")]
        public async Task<List<SpawnPoint>> GetAllPokemonSpawns(MapQuery query)
        {
            using (var ctx = new PokemonDb())
            {
                using (var dbContextTransaction = ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    DbGeography box = DbGeography.PolygonFromText(
                    string.Format(System.Globalization.CultureInfo.InvariantCulture, "POLYGON(({0} {1},{2} {1},{2} {3},{0} {3},{0} {1}))",
                                 query.swLng,
                                 query.swLat,
                                 query.neLng,
                                 query.neLat
                        ), 4326);
                    var sp = ctx.SpawnPoints.Include("Encounters").Where(x => x.Location.Intersects(box) && x.Encounters.Any(a => query.pokemonIds.Contains(a.PokemonId)));
                    var spR = await sp.Select(x => new SpawnPoint() { Id = x.Id, Latitude = x.Location.Latitude.Value, Longitude = x.Location.Longitude.Value, Encounters = x.Encounters.Where(b => query.pokemonIds.Contains(b.PokemonId)).OrderBy(b => b.EstimatedSpawnTime).Select(a => new Encounter() { PokemonId = a.PokemonId, PokemonName = a.Pokemon.LabelFr, SpawnTime = a.EstimatedSpawnTime }).ToList() }).ToListAsync();
                    dbContextTransaction.Commit();
                    return spR;
                }
            }
        }

        private string FormatExceptionMessage(Exception ex)
        {
            string exMessage = ex.Message + "\r\n" + ex.StackTrace + "\r\n\r\n";
            if (ex.InnerException != null)
                exMessage += FormatExceptionMessage(ex.InnerException);
            return exMessage;
        }
        [HttpPost]
        [ActionName("PostEncounter")]
        public BaseResponse PostEncounter(SpawnPointsImport import)
        {
            try
            {
                //return new BaseResponse() { Success = true, Message = "Functionality disabled for the next hour while I'm analyzing some performance issues" };
                if (ModelState.IsValid)
                {
                    lock (Globals.DumpQueueLock)
                    {
                        Globals.DumpQueue.AddRange(import.SpawnPoints);
                    }
                    return new BaseResponse() { Success = true, Message = import.SpawnPoints.SelectMany(x => x.Encounters).Count() + " pokemons successfully added in the serveur Queue" };
                }
                else
                {

                    return new BaseResponse() { Success = false, Message = "You are using an old version of the scanner, please update it." };
                    //Retrieve the exceptions raised during deserialization
                    //var errors = ModelState.SelectMany(v => v.Value.Errors.Select(e => e.Exception));

                    //List<String> messages = new List<string>();

                    //foreach (Exception e in errors)
                    //{
                    //    messages.Add(e.GetType().ToString() + ": " + e.Message);
                    //}

                    //return new BaseResponse() { Success = false, Message = string.Join("\r\n", messages) };
                }
            }
            catch (Exception e)
            {
                return new BaseResponse() { Success = true, Message = "Sorry an Error occured, we're trying new stuff to improve performance of the server, please keep scanning" };
            }

        }
        [HttpGet]
        [ActionName("GetCells")]
        public CellResponse GetCells(string latLng)
        {
            return new CellResponse();
            var bounds = latLng.Split(',').Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            var region_rect = S2LatLngRect.FromPointPair(
                S2LatLng.FromDegrees(bounds[0], bounds[1]),
                S2LatLng.FromDegrees(bounds[2], bounds[3]));
            var coverer = new S2RegionCoverer() { MaxLevel = 16, MinLevel = 16, LevelMod = 0, MaxCells = int.MaxValue };
            var covering = new List<S2CellId>();
            coverer.GetCovering(region_rect, covering);
            var cellsToCheck = S2Helper.GetListOfCellsToCheck(covering).ToDictionary(x => x.Id);
            var result = new CellResponse();

            foreach (var cellId in covering)
            {
                var rCell = new Cell(cellId);
                result.Cells.Add(rCell);
                if (cellsToCheck.ContainsKey(cellId.Id))
                    result.Centers.Add(new LatLng() { lat = cellId.Center().LatDegrees, lng = cellId.Center().LngDegrees });
            }
            return result;
        }
        [HttpGet]
        [ActionName("GetCells2")]
        public CellResponse GetCells2()
        {
            var result = new List<Cell>();
            var tt = S2Helper.GetNearbyCellIds(48.91598573367739, 2.540661974689442);
            var center = S2CellId.FromLatLng(S2LatLng.FromDegrees(48.91598573367739, 2.540661974689442)).ParentForLevel(15);
            var centersId = new List<ulong>();

            var res = new CellResponse();

            foreach (var cellId in tt)
            {
                var rCell = new Cell() { Id = cellId.ToString(), Shape = new List<LatLng>() };
                if (centersId.Contains(cellId)) rCell.Center = true;
                var cell = new S2Cell(new S2CellId(cellId));
                for (var i = 0; i < 4; i++)
                {
                    var latLng = new S2LatLng(cell.GetVertex(i));
                    rCell.Shape.Add(new LatLng() { lat = latLng.LatDegrees, lng = latLng.LngDegrees });
                }
                res.Cells.Add(rCell);
                var centerLatLng = new S2CellId(cellId).Center();
                res.Centers.Add(new LatLng() { lat = centerLatLng.LatDegrees, lng = centerLatLng.LngDegrees });
            }
            return res;

        }
        [HttpGet]
        [ActionName("GetCells3")]
        public CellResponse GetCells3()
        {
            var resP = new CellResponse() { Cells = new List<Cell>(), Centers = new List<LatLng>() };
            var result = resP.Cells;
            var mapManager = Globals.MapManager;
            foreach (var cellId in mapManager.MapsCells)
            {

                var rCell = new Cell() { Id = cellId.Key.ToString(), Shape = new List<LatLng>() };
                if (cellId.Value.WildPokemons.Any() || cellId.Value.CatchablePokemons.Any()) rCell.Center = true;
                var cell = new S2Cell(new S2CellId(cellId.Key));
                for (var i = 0; i < 4; i++)
                {
                    var latLng = new S2LatLng(cell.GetVertex(i));
                    rCell.Shape.Add(new LatLng() { lat = latLng.LatDegrees, lng = latLng.LngDegrees });
                }
                result.Add(rCell);
            }
            return resP;
        }
        [HttpGet]
        [ActionName("GetCells4")]
        public CellResponse GetCells4(string latLng,int? hexNum = null)
        {
            var parsed = latLng.Split(',');
            double lat = double.Parse(parsed[0],System.Globalization.CultureInfo.InvariantCulture);
            double lng = double.Parse(parsed[1],System.Globalization.CultureInfo.InvariantCulture);
            double FLOAT_LAT = 0;
            double FLOAT_LNG = 0;
            var cellResponse = new CellResponse();
            int wID = 0;
            int HEX_NUM = hexNum.HasValue? hexNum.Value : 20;
            var latrad = lat * Math.PI / 180;
            var ab = (HEX_NUM + 0.5);
            var x_un = 1.5 * HEX_R / GetEarthRadius(latrad) / Math.Cos(latrad) * safety * ab * 180 / Math.PI;
            var y_un = 3.0 * HEX_M / GetEarthRadius(latrad) * safety * ab * 180 / Math.PI;
            var xmod = new int[] { 0, 1, 2, 1, -1, -2, -1 };
            var ymod = new int[] { 0, -1, 0, 1, 1, 0, -1 };
            lat = lat + ymod[wID] * y_un;
            lng = lng + xmod[wID] * x_un;
            FLOAT_LAT = lat;
            FLOAT_LNG = lng;
            cellResponse.Centers.Add(new LatLng() { lat = lat, lng = lng });
            latrad = lat * Math.PI / 180;
            int maxR = 1;
            for (var a = 1; a < HEX_NUM + 1; a++)
                for (var i = 0; i < (a * 6); i++)
                {
                    x_un = 1.5 * HEX_R / GetEarthRadius(latrad) / Math.Cos(latrad) * safety * 180 / Math.PI;
                    y_un = 1.0 * HEX_M / GetEarthRadius(latrad) * safety * 180 / Math.PI;
                    if (i < a)
                    {
                        lat = FLOAT_LAT + y_un * (-2 * a + i);
                        lng = FLOAT_LNG + x_un * i;
                    }
                    else if (i < 2 * a)
                    {
                        lat = FLOAT_LAT + y_un * (-3 * a + 2 * i);
                        lng = FLOAT_LNG + x_un * a;
                    }
                    else if (i < 3 * a)
                    {
                        lat = FLOAT_LAT + y_un * (-a + i);
                        lng = FLOAT_LNG + x_un * (3 * a - i);
                    }
                    else if (i < 4 * a)
                    {
                        lat = FLOAT_LAT + y_un * (5 * a - i);
                        lng = FLOAT_LNG + x_un * (3 * a - i);
                    }
                    else if (i < 5 * a)
                    {
                        lat = FLOAT_LAT + y_un * (9 * a - 2 * i);
                        lng = FLOAT_LNG + x_un * -a;
                    }
                    else
                    {
                        lat = FLOAT_LAT + y_un * (4 * a - i);
                        lng = FLOAT_LNG + x_un * (-6 * a + i);
                    }
                    cellResponse.Centers.Add(new LatLng() { lat = lat, lng = lng });
                    maxR += 1;
                }
            return cellResponse;
        }
        private double GetEarthRadius(double latrad)
        {
            return Math.Pow((1.0 / (Math.Pow(((Math.Cos(latrad)) / EARTH_Rmax), 2) + Math.Pow(((Math.Sin(latrad)) / EARTH_Rmin), 2))), (1.0 / 2));
        }
        //Test Code again :O
        static private double safety = 1.0;
        
        static private double EARTH_Rmax = 6378137.0;
        static private double EARTH_Rmin = 6356752.3;
        static private double HEX_R = 100.0;//range of detection for pokemon = 100m
        static private double HEX_M = Math.Pow(3.0, 0.5) / 2.0 * HEX_R;
        static private double ALT_C = 5;
    }
}

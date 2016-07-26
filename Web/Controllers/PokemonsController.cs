using Google.Common.Geometry;
using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models.WebModels.Requests;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web.Controllers
{
    public class PokemonsController : ApiController
    {
        [HttpPost]
        [ActionName("ListAllNew")]
        public async Task<List<MapPokemon>> ListAll2(MapQuery query)
        {
            var region_rect = S2LatLngRect.FromPointPair(
                   S2LatLng.FromDegrees(query.neLat, query.neLng),
                   S2LatLng.FromDegrees(query.swLat,query.swLng));
            var coverer = new S2RegionCoverer() { MaxLevel = 13, MinLevel = 13, LevelMod = 0, MaxCells = int.MaxValue };
            var covering = new List<S2CellId>();
            coverer.GetCovering(region_rect, covering);

            while (Globals.IsUpdatingLivePokemons)
                await Task.Delay(10);

            var res = covering.Where(x=> Globals.LivePokemons.ContainsKey(x.Id)).SelectMany(x => Globals.LivePokemons[x.Id].Values).ToList();
            res.RemoveAll(x => query.pokemonIds.Contains(x.PokedexNumber));
            res.ForEach(x => x.Name = Globals.PokemonNamesById[x.PokedexNumber][query.lang]);
            return res;
        }

        [HttpPost]
        [ActionName("ListAll")]
        public async Task<List<MapPokemon>> ListAll(MapQuery query)
        {
            using (var ctx = new PokemonDb())
            {
                using (var dbContextTransaction = ctx.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    ctx.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                    DbGeography box = DbGeography.PolygonFromText(
                     string.Format(System.Globalization.CultureInfo.InvariantCulture, "POLYGON(({0} {1},{2} {1},{2} {3},{0} {3},{0} {1}))",
                                  query.swLng,
                                  query.swLat,
                                  query.neLng,
                                  query.neLat
                         ), 4326);
                    var queryDb =  ctx.SpawnPoints.Where(x=> x.Location.Intersects(box)).SelectMany(a => a.Encounters.Where(x=> x.ExpirationTime > DateTime.UtcNow).Select(x=>  new MapPokemon() { ExpirationTime = x.ExpirationTime, Latitude = x.SpawnPoint.Location.Latitude, Longitude = x.SpawnPoint.Location.Longitude, PokedexNumber = x.PokemonId, EncounterId = x.Id}));
                    var res = await queryDb.ToListAsync();
                    res.RemoveAll(x => query.pokemonIds.Contains(x.PokedexNumber));
                    res.ForEach(x => x.Name = Globals.PokemonNamesById[x.PokedexNumber][query.lang]);
                    return res;
                }
            }
        }
        [HttpGet]
        public List<MapPokemon> GetPokemonForName(string start,string lang="fr")
        {            
            return Globals.PokemonNamesByLang[lang].Where(x => x.Value.ToLowerInvariant().StartsWith(start.ToLowerInvariant())).Select(x => new MapPokemon() { PokedexNumber = x.Key, Name = x.Value }).ToList();
        }
    }
}

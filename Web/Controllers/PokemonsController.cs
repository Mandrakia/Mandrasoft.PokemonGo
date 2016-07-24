using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models.WebModels.Requests;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web.Controllers
{
    public class PokemonsController : ApiController
    {

        [HttpPost]
        public async Task<List<MapPokemon>> ListAll(MapQuery query)
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
                    var res = await ctx.Encounters.Where(x => !query.pokemonIds.Contains(x.PokemonId) && x.SpawnPoint.Location.Intersects(box) && x.ExpirationTime > DateTime.UtcNow)
                            .Select(x => new MapPokemon() { ExpirationTime = x.ExpirationTime, Latitude = x.SpawnPoint.Location.Latitude, Longitude = x.SpawnPoint.Location.Longitude, PokedexNumber = x.PokemonId, EncounterId = x.Id}).ToListAsync();
                    res.ForEach(x => x.Name = Globals.PokemonNamesById[x.PokedexNumber][query.lang]);
                    return res;
                }
            }
        }
        [HttpGet]
        public List<MapPokemon> GetPokemonForName(string start,string lang="fr")
        {            
            return Globals.PokemonNamesByLang[lang].Where(x => x.Value.ToLowerInvariant().StartsWith(start)).Select(x => new MapPokemon() { PokedexNumber = x.Key, Name = x.Value }).ToList();
        }
    }
}

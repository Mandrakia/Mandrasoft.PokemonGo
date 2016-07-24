using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using MandraSoft.PokemonGo.Models.WebModels.Requests;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using POGOProtos.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web.Controllers
{
    public class AuthenticationController : ApiController
    {
        [HttpPost]
        public async Task<List<MapPokemon>> ListMyPokemons(LoginContainer login)
        {
            using (var client = new PokemonGoClient())
            {
                await client.LoginGoogle(login.Login, login.Password);
                await client.SetServer();
                var inventory = await client.UpdateInventory();
                return inventory.InventoryDelta.InventoryItems.Where(x => x.InventoryItemData.PokemonData != null && x.InventoryItemData.PokemonData.PokemonId != PokemonId.Missingno).Select(x => (int)x.InventoryItemData.PokemonData.PokemonId).Distinct().Select(x => new MapPokemon() { PokedexNumber = x, Name = Globals.PokemonNamesById[x][login.lang] }).ToList();
            }
        }
    }
}

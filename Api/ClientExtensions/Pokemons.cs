using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Networking.Responses;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Requests;
using Google.Protobuf;
using MandraSoft.PokemonGo.Api.Helpers;
using MandraSoft.PokemonGo.Api.Extensions;

namespace MandraSoft.PokemonGo.Api.ClientExtensions
{
    static public class Pokemons
    {
        static internal Request GetEvolvePokemonRequest(this PokemonGoClient client,ulong pokemonId)
        {
            var msg = new EvolvePokemonMessage()
            {
                PokemonId = pokemonId
            };
            return new Request() { RequestType = RequestType.EvolvePokemon, RequestMessage = msg.ToByteString() };
        }
        static public async Task<EvolvePokemonResponse> GetEvolvePokemonResponse(this PokemonGoClient client, ulong pokemonId)
        {
            return (EvolvePokemonResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetEvolvePokemonRequest(pokemonId)))[0];
        }
        static internal Request GetReleasePokemonRequest(this PokemonGoClient client, ulong pokemonId)
        {
            var msg = new ReleasePokemonMessage
            {
                PokemonId = pokemonId
            };
            return new Request() { RequestType = RequestType.ReleasePokemon, RequestMessage = msg.ToByteString() };
        }
        static public async Task<ReleasePokemonResponse> GetReleasePokemonResponse(this PokemonGoClient client, ulong pokemonId)
        {
            return (ReleasePokemonResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetReleasePokemonRequest(pokemonId)))[0];
        }
    }
}
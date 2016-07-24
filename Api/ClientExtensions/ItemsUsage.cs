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
using POGOProtos.Inventory;

namespace MandraSoft.PokemonGo.Api.ClientExtensions
{
    static public class ItemsUsage
    {
        static public Request GetUseItemXpBoostRequest(this PokemonGoClient client,ItemId itemId)
        {
            var msg = new UseItemXpBoostMessage()
            {
                ItemId = itemId
            };
            return new Request() { RequestType = RequestType.UseItemXpBoost, RequestMessage = msg.ToByteString() };
        }
        static public async Task<UseItemXpBoostResponse> GetUseItemXpBoostResponse(this PokemonGoClient client, ItemId itemId)
        {
            return (UseItemXpBoostResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl, client.GetUseItemXpBoostRequest(itemId)))[0];
        }
    }
}
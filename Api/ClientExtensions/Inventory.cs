using Google.Protobuf;
using MandraSoft.PokemonGo.Api.Extensions;
using POGOProtos.Inventory;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.ClientExtensions
{
    static public class Inventory
    {
        static public Request GetRecycleInventoryItemRequest(this PokemonGoClient client,ItemId itemId,int count)
        {
            RecycleInventoryItemMessage msg = new RecycleInventoryItemMessage()
            {
                Count = count,
                ItemId = itemId
            };
            return new Request() { RequestType = RequestType.RecycleInventoryItem, RequestMessage = msg.ToByteString() };
        }
        static public async Task<RecycleInventoryItemResponse> GetRecycleInventoryItemResponse(this PokemonGoClient client, ItemId itemId, int count)
        {
            return (RecycleInventoryItemResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl, client.GetRecycleInventoryItemRequest(itemId, count)))[0];
        }
        static public Request GetInventoryRequest(this PokemonGoClient client, long lastTimestamp = -1)
        {
            if (lastTimestamp == -1) lastTimestamp = client.InventoryManager.LastTimestamp;
            var t = new GetInventoryMessage() { LastTimestampMs = lastTimestamp };
            return new Request() { RequestType = RequestType.GetInventory, RequestMessage = t.ToByteString() };
        }
        static public async Task UpdateInventory(this PokemonGoClient client)
        {
            await client._httpClient.GetResponses(client, true, client._apiUrl,client.GetInventoryRequest());
        }
    }
}

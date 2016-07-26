using Google.Protobuf;
using MandraSoft.PokemonGo.Api.Logging;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.ClientExtensions
{
    static public class GenericResponseHandlers
    {
        static public Dictionary<Type, ResponseHandler> GenericHandlers = new Dictionary<Type, ResponseHandler>() {
            { typeof(GetInventoryResponse), HandleInventoryResponse },
            {typeof(CheckAwardedBadgesResponse),HandleBadgeResponses },
            {typeof(GetHatchedEggsResponse),HandleEggHatchedResponse },
            {typeof(GetMapObjectsResponse),HandleMapObjectsResponse }
        };
        public delegate Task ResponseHandler(PokemonGoClient client, IMessage message);
        static public async Task HandleGenericResponses(this PokemonGoClient client, List<IMessage> responses)
        {
            foreach (var message in responses)
            {
                if (GenericHandlers.ContainsKey(message.GetType()))
                    await GenericHandlers[message.GetType()](client, message);
            }
        }

        static public async Task HandleMapObjectsResponse(PokemonGoClient client, IMessage response)
        {
            var resp = (GetMapObjectsResponse)response;
            client.MapManager.UpdateMapCells(resp);
            if (client.MapObjectsHandler != null)
                await client.MapObjectsHandler(client, resp);
        }

        static public async Task HandleInventoryResponse(PokemonGoClient client, IMessage response)
        {
            var msg = (GetInventoryResponse)response;
            client.InventoryManager.UpdateItems(msg);
        }
        //TODO: Provide Public Callback for it.
        static public async Task HandleBadgeResponses(PokemonGoClient client, IMessage response)
        {
            var msg = (CheckAwardedBadgesResponse)response;
            foreach (var x in msg.AwardedBadges)
            {
                Logger.Write($"Awarded a badge: {x}");
            }
        }
        //TODO: Provide PublicCallBackForIt.
        static public async Task HandleEggHatchedResponse(PokemonGoClient client, IMessage response)
        {
            var msg = (GetHatchedEggsResponse)response;

            foreach (var x in msg.PokemonId)
            {
                Logger.Write($"An egg hatched! You received: {x}");
            }

            if (msg.PokemonId.Any())
            {
                Logger.Write($"Total XP Awarded: {msg.ExperienceAwarded.Sum()}");
                Logger.Write($"Total stardust Awarded: {msg.StardustAwarded.Sum()}");
            }
        }
    }
}

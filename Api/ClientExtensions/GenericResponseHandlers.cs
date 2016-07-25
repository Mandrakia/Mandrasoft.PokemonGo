using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POGOProtos.Networking.Responses;

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
                Console.WriteLine("Awarded a badge : " + x);
            }
        }
        //TODO: Provide PublicCallBackForIt.
        static public async Task HandleEggHatchedResponse(PokemonGoClient client, IMessage response)
        {
            var msg = (GetHatchedEggsResponse)response;
            
            foreach (var x in msg.PokemonId)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("An egg hatched ! You received : " + x );
                Console.ResetColor();
            }
            if (msg.PokemonId.Any())
            {
                Console.WriteLine("Total XP Awarded : " + msg.ExperienceAwarded.Sum());
                Console.WriteLine("Total stardust Awarded : " + msg.StardustAwarded.Sum());
            }
        }

    }
}

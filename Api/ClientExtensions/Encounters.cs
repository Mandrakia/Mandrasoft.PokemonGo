using Google.Protobuf;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using MandraSoft.PokemonGo.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Inventory;

namespace MandraSoft.PokemonGo.Api.ClientExtensions
{
    static public class Encounters
    {
        static internal Request GetUseItemCaptureRequest(this PokemonGoClient client, ItemId itemType,ulong encounterId,string spawnId)
        {
            var msg = new UseItemCaptureMessage()
            {
                EncounterId = encounterId,
                SpawnPointGuid = spawnId,
                ItemId = itemType
            };
            return new Request() { RequestType = RequestType.UseItemCapture, RequestMessage = msg.ToByteString() };              
        }
        static public async Task<UseItemCaptureResponse> GetUseItemCaptureResponse(this PokemonGoClient client, ItemId itemType, ulong encounterId, string spawnId)
        {
            return (UseItemCaptureResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetUseItemCaptureRequest(itemType, encounterId, spawnId)))[0];
        }
        static internal Request GetEncounterRequest(this PokemonGoClient client, ulong encounterId, string spawnId)
        {
            var msg = new EncounterMessage()
            {
                EncounterId = encounterId,
                PlayerLatitude = client.Latitude,
                PlayerLongitude = client.Longitude,
                SpawnPointId = spawnId
            };
            return new Request() { RequestType = RequestType.Encounter, RequestMessage = msg.ToByteString() };
        }
        static public async Task<EncounterResponse> GetEncounterResponse(this PokemonGoClient client, ulong encounterId, string spawnId)
        {
            return (EncounterResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetEncounterRequest(encounterId, spawnId)))[0];
        }

        static public async Task<GetIncensePokemonResponse> GetIncensePokemonsResponse(this PokemonGoClient client)
        {
            return (GetIncensePokemonResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetIncensePokemonRequest()))[0];
        }
        static internal Request GetIncensePokemonRequest(this PokemonGoClient client)
        {
            var msg = new GetIncensePokemonMessage()
            {
                PlayerLatitude = client.Latitude,
                PlayerLongitude = client.Longitude
            };
            return new Request() { RequestType = RequestType.GetIncensePokemon, RequestMessage = msg.ToByteString() };                
        }

        static internal Request GetIncenseEncounterRequest(this PokemonGoClient client,long encounterId,string encounterLocation)
        {
            var msg = new IncenseEncounterMessage()
            {
                EncounterId = encounterId,
                EncounterLocation = encounterLocation
            };
            return new Request() { RequestType = RequestType.IncenseEncounter, RequestMessage = msg.ToByteString() };
        }
        static public async Task<IncenseEncounterResponse> GetIncenseEncounterResponse(this PokemonGoClient client, long encounterId, string encounterLocation)
        {
            return (IncenseEncounterResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetIncenseEncounterRequest(encounterId, encounterLocation)))[0];
        }

        static internal Request GetCatchPokemonRequest(this PokemonGoClient client, ulong encounterId, string spawnpoint, ItemId pokeType)
        {
            var msg = new CatchPokemonMessage()
            {
                EncounterId = encounterId,
                Pokeball = (int)pokeType,
                SpawnPointGuid = spawnpoint,
                HitPokemon = true,
                NormalizedReticleSize = 1.950,
                SpinModifier = 1,
                NormalizedHitPosition = 1
            };
            return new Request() { RequestType = RequestType.CatchPokemon, RequestMessage = msg.ToByteString() };
        }
        static public async Task<CatchPokemonResponse> GetCatchPokemonResponse(this PokemonGoClient client, ulong encounterId, string spawnpoint,ItemId pokeType)
        { 
            return (CatchPokemonResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl,null,null, client.GetCatchPokemonRequest(encounterId, spawnpoint, pokeType)))[0];
        }
    }
}

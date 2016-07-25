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
    static public class Map
    {

        static internal Request GetMapRequest(this PokemonGoClient client, IList<ulong> CellIds = null,double? latitude=null,double? longitude = null)
        {
            double requestedLat = latitude.HasValue ? latitude.Value : client.Latitude;
            double requestedLng = longitude.HasValue ? longitude.Value : client.Longitude;
            var customRequest = new GetMapObjectsMessage()
            {
                Latitude = requestedLat,
                Longitude = requestedLng
            };
            if (CellIds == null)
                CellIds = S2Helper.GetNearbyCellIds(requestedLat, requestedLng);
            customRequest.CellId.Add(CellIds);
            customRequest.SinceTimestampMs.AddRange(client.MapManager.GetLastUpdatedTimestamp(CellIds));
            return new Request() { RequestType = RequestType.GetMapObjects, RequestMessage = customRequest.ToByteString() };
        }
        static public async Task UpdateMapObjects(this PokemonGoClient client,double? latitude = null,double? longitude = null,IList<ulong> CellIds = null)
        {
            using (var httpClient = client.GetHttpClient())
            {
                var res = await httpClient.GetResponses(client, true, client._apiUrl,
                    client.GetMapRequest(CellIds, latitude, longitude),
                    client.GetHatchedEggRequest(),
                    client.GetInventoryRequest(),
                    client.GetCheckAwardedBadgeRequest(),
                    client.GetDownloadSettingsRequest());
            }
        }

        static internal Request GetPlayerUpdateRequest(this PokemonGoClient client)
        {
            var msg = new PlayerUpdateMessage()
            {
                Latitude = client.Latitude,
                Longitude = client.Longitude
            };
            return new Request() { RequestType = RequestType.PlayerUpdate, RequestMessage = msg.ToByteString() };
        }
        static public async Task<PlayerUpdateResponse> GetPlayerUpdateResponse(this PokemonGoClient client)
        {
            return (PlayerUpdateResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl, client.GetPlayerUpdateRequest()))[0];
        }
        static public async Task<PlayerUpdateResponse> GetPlayerUpdateResponse(this PokemonGoClient client, double lat, double lng)
        {
            PlayerUpdateResponse result = null;
           
                client.SetCoordinates(lat, lng);
                result = await client.GetPlayerUpdateResponse();
            
            return result;
        }

        static internal Request GetFortSearchRequest(this PokemonGoClient client, string fortId, double fortLat, double fortLng)
        {
            var msg = new FortSearchMessage()
            {
                FortId = fortId,
                FortLatitude = fortLat,
                FortLongitude = fortLng,
                PlayerLatitude = client.Latitude,
                PlayerLongitude = client.Longitude
            };
            return new Request() { RequestType = RequestType.FortSearch, RequestMessage = msg.ToByteString() };
        }
        static public async Task<FortSearchResponse> GetFortSearchResponse(this PokemonGoClient client, string fortId, double fortLat, double fortLng)
        {
            return (FortSearchResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl, client.GetFortSearchRequest(fortId, fortLat, fortLng)))[0];
        }

        static internal Request GetFortDetailsRequest(this PokemonGoClient client, string fortId, double fortLat, double fortLng)
        {
            var msg = new FortDetailsMessage()
            {
                FortId = fortId,
                Latitude = fortLat,
                Longitude = fortLng
            };
            return new Request() { RequestType = RequestType.FortDetails, RequestMessage = msg.ToByteString() };
        }
        static public async Task<FortDetailsResponse> GetFortDetailsResponse(this PokemonGoClient client, string fortId, double fortLat, double fortLng)
        {
            return (FortDetailsResponse)(await client._httpClient.GetResponses(client, true, client._apiUrl, client.GetFortDetailsRequest(fortId, fortLat, fortLng)))[0];
        }
    }
}

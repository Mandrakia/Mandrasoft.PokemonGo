using Google.Protobuf;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using POGOProtos.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Extensions
{
    public static class HttpClientExtensions
    {
        private static bool Relogging = false;
        public static async Task<List<IMessage>> GetResponses(this HttpClient client, PokemonGoClient pogoClient, bool withAuthTicket, string url, params Request[] requests)
        {
            var response = await GetEnvelope(client, pogoClient, withAuthTicket, url, requests);
            int retryCount = 0;
            while (response.Returns.Count != requests.Length)
            {
                response = await GetEnvelope(client, pogoClient, withAuthTicket, url, requests);
                await Task.Delay(500);
                retryCount++;
                if (retryCount == 5)
                {
                    if (!Relogging)
                    {
                        Relogging = true;
                        try
                        {
                            await pogoClient.Login();
                            await pogoClient.SetServer();
                        }
                        catch { }
                        finally
                        { Relogging = false; }
                    };

                }
                if (retryCount > 5) throw new Exception("Client in a weird state");
            }
            var result = new List<IMessage>();
            for (var i = 0; i < requests.Length; i++)
            {
                Type T = Globals.GetResponseTypeForRequestType(requests[i].RequestType);
                if (T != null)
                {
                    var n = (IMessage)Activator.CreateInstance(T);
                    n.MergeFrom(response.Returns[i]);
                    result.Add(n);
                }
                else result.Add(new POGOProtos.Networking.Responses.EchoResponse());
            }
            await pogoClient.HandleGenericResponses(result);
            return result;
        }

        public static async Task<POGOProtos.Networking.Envelopes.ResponseEnvelope> GetEnvelope(this HttpClient client, PokemonGoClient pogoClient, bool withAuthTicket, string url, params Request[] requests)
        {
            //Check Session closed.
            if (withAuthTicket && pogoClient._authTicket.ExpireTimestampMs < (ulong)DateTime.UtcNow.ToUnixTime())
            {
                await pogoClient.LoginPtc();
                await pogoClient.SetServer();
                url = pogoClient._apiUrl;
            }
            var response = new POGOProtos.Networking.Envelopes.ResponseEnvelope();
            var requestEnvloppe = await pogoClient.GetRequest(withAuthTicket, requests);
            var data = requestEnvloppe.ToByteString();
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));

            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            response.MergeFrom(codedStream);
            return response;
        }
    }
}

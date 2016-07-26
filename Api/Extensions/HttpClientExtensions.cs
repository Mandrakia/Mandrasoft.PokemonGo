using Google.Protobuf;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using POGOProtos.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Extensions
{
    public static class HttpClientExtensions
    {
        private static bool Relogging = false;
        static int _Count = 0;
        static long _TotalElapsedNetwork = 0;
        static long _TotalElapsedHandling = 0; 
        public static async Task<List<IMessage>> GetResponses(this HttpClient client, PokemonGoClient pogoClient, bool withAuthTicket, string url,double? latitude,double? longitude, params Request[] requests)
        {
#if _INSTRUMENTING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            var response = await GetEnvelope(client, pogoClient, withAuthTicket, url,latitude,longitude, requests);
#if _INSTRUMENTING
            _TotalElapsedNetwork += sw.ElapsedMilliseconds;
#endif
            int retryCount = 0;
            while (response.Returns.Count != requests.Length)
            {
                response = await GetEnvelope(client, pogoClient, withAuthTicket, url,latitude,longitude, requests);
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
#if _INSTRUMENTING
            sw.Reset();
            sw.Start();
#endif
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
#if _INSTRUMENTING
            _TotalElapsedHandling += sw.ElapsedMilliseconds;
            _Count++;
            if (_Count % 100 == 0)
            {
                Logger.Write("Average network time : " + (double)_TotalElapsedNetwork / (double)_Count + "ms");
                Logger.Write("     AuthTicket : " + (double)_TotalAuthTicket / (double)_Count + "ms");
                Logger.Write("     Request Serialization : " + (double)_TotalSerializationRequest / (double)_Count + "ms");
                Logger.Write("     Post Time : " + (double)_TotalPostTime / (double)_Count + "ms");
                Logger.Write("     ReadResponse : " + (double)_TotalReadResponse / (double)_Count + "ms");
                Logger.Write("     Deserialization : " + (double)_TotalDeserialization / (double)_Count + "ms");
                Logger.Write("Average handling time : " + (double)_TotalElapsedHandling / (double)_Count + "ms");
            }
#endif
            return result;
        }
        static private long _TotalAuthTicket = 0;
        static private long _TotalSerializationRequest = 0;
        static private long _TotalPostTime = 0;
        static private long _TotalReadResponse = 0;
        static private long _TotalDeserialization = 0;
        public static async Task<POGOProtos.Networking.Envelopes.ResponseEnvelope> GetEnvelope(this HttpClient client, PokemonGoClient pogoClient, bool withAuthTicket, string url,double? latitude,double? longitude, params Request[] requests)
        {
            #if _INSTRUMENTING
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            //Check Session closed.
            if (withAuthTicket && pogoClient._authTicket.ExpireTimestampMs < (ulong)DateTime.UtcNow.ToUnixTime())
            {
                await pogoClient.LoginPtc();
                await pogoClient.SetServer();
                url = pogoClient._apiUrl;
            }
#if _INSTRUMENTING
            _TotalAuthTicket += sw.ElapsedMilliseconds;
            sw.Restart();
#endif
            var response = new POGOProtos.Networking.Envelopes.ResponseEnvelope();
            var requestEnvloppe = await pogoClient.GetRequest(withAuthTicket,latitude,longitude, requests);
            var data = requestEnvloppe.ToByteString();
#if _INSTRUMENTING
            _TotalSerializationRequest += sw.ElapsedMilliseconds;
            sw.Restart();
#endif
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));
#if _INSTRUMENTING
            _TotalPostTime += sw.ElapsedMilliseconds;
            sw.Restart();
#endif
            var responseData = await result.Content.ReadAsByteArrayAsync();
#if _INSTRUMENTING
            _TotalReadResponse += sw.ElapsedMilliseconds;
            sw.Restart();
#endif
            var codedStream = new CodedInputStream(responseData);
            response.MergeFrom(codedStream);
#if _INSTRUMENTING
            _TotalDeserialization += sw.ElapsedMilliseconds;
#endif
            return response;
        }
    }
}

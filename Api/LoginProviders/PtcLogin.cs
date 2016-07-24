using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MandraSoft.PokemonGo.Api.LoginProviders
{
    internal static class PtcLogin
    {
        public static string GetValue(string json, string key)
        {
            var jObject = JObject.Parse(json);
            return jObject[key].ToString();
        }
        public static async Task<string> GetAccessToken(string username, string password)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var tempHttpClient = new HttpClient(handler))
            {
                //Get session cookie
                var sessionResp = await tempHttpClient.GetAsync(Globals.PtcLoginUrl);
                var data = await sessionResp.Content.ReadAsStringAsync();
                var lt = GetValue(data, "lt");
                var executionId = GetValue(data, "execution");

                //Login
                var loginResp = await tempHttpClient.PostAsync(Globals.PtcLoginUrl,
                    new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("lt", lt),
                            new KeyValuePair<string, string>("execution", executionId),
                            new KeyValuePair<string, string>("_eventId", "submit"),
                            new KeyValuePair<string, string>("username", username),
                            new KeyValuePair<string, string>("password", password),
                        }));

                var ticketId = HttpUtility.ParseQueryString(loginResp.Headers.Location.Query)["ticket"];

                //Get tokenvar 
                var tokenResp = await tempHttpClient.PostAsync(Globals.PtcLoginOauth,
                    new FormUrlEncodedContent(
                        new[]
                        {
                            new KeyValuePair<string, string>("client_id", "mobile-app_pokemon-go"),
                            new KeyValuePair<string, string>("redirect_uri",
                                "https://www.nianticlabs.com/pokemongo/error"),
                            new KeyValuePair<string, string>("client_secret",
                                "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"),
                            new KeyValuePair<string, string>("grant_type", "refresh_token"),
                            new KeyValuePair<string, string>("code", ticketId),
                        }));

                var tokenData = await tokenResp.Content.ReadAsStringAsync();
                return HttpUtility.ParseQueryString(tokenData)["access_token"];
            }
        }
    }
}

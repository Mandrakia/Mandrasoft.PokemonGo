using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.LoginProviders
{
    static public class GoogleLogin
    {
        internal static async Task<string> LoginGoogle(string username, string password)
        {
            var first = "https://accounts.google.com/o/oauth2/auth?client_id=848232511240-73ri3t7plvk96pj4f85uj8otdat2alem.apps.googleusercontent.com&redirect_uri=urn%3Aietf%3Awg%3Aoauth%3A2.0%3Aoob&response_type=code&scope=openid%20email%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email";
            var second = "https://accounts.google.com/AccountLoginInfo";
            var third = "https://accounts.google.com/signin/challenge/sl/password";
            var last = "https://accounts.google.com/o/oauth2/token";
            using (var clientHandler = new HttpClientHandler())
            {
                clientHandler.AllowAutoRedirect = true;
                using (var client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPad; CPU OS 8_4 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Mobile/12H143");
                    var response = await client.GetAsync(first);
                    var r = await response.Content.ReadAsStringAsync();

                    var galx_regex = "name=\"GALX\" value=\"(.*?)\"";
                    var matches = Regex.Matches(r, galx_regex);
                    var galx = matches[0].Groups[1].Value;
                    var gxf_regex = "name=\"gxf\" value=\"(.*?)\"";
                    var gxf = Regex.Matches(r, gxf_regex)[0].Groups[1].Value;
                    var cont_regex = "name=\"continue\" value=\"(.*?)\"";
                    matches = Regex.Matches(r, cont_regex);
                    var cont = matches[0].Groups[1].Value.Replace("&amp;", "&");
                    var data1 = new[]
                    {
                        new KeyValuePair<string, string>("Page", "PasswordSeparationSignIn"),
                        new KeyValuePair<string, string>("GALX", galx),
                        new KeyValuePair<string, string>("gxf", gxf),
                        new KeyValuePair<string, string>("continue", cont),
                        new KeyValuePair<string, string>("ltmpl", "embedded"),
                        new KeyValuePair<string, string>("scc", "1"),
                        new KeyValuePair<string, string>("sarp", "1"),
                        new KeyValuePair<string, string>("oauth", "1"),
                        new KeyValuePair<string, string>("ProfileInformation", ""),
                        new KeyValuePair<string, string>("_utf8", "?"),
                        new KeyValuePair<string, string>("bgresponse", "js_disabled"),
                        new KeyValuePair<string, string>("Email", username),
                        new KeyValuePair<string, string>("signIn", "Next"),
                    };
                    response = await client.PostAsync(second, new FormUrlEncodedContent(data1));
                    r = await response.Content.ReadAsStringAsync();
                    gxf = Regex.Matches(r, gxf_regex)[0].Groups[1].Value;
                    var profileinformation_regex = "name=\"ProfileInformation\" type=\"hidden\" value=\"(.*?)\"";
                    var profileinformation = Regex.Matches(r, profileinformation_regex)[0].Groups[1].Value;
                    var data2 = new[]
                    {
                        new KeyValuePair<string, string>("Page", "PasswordSeparationSignIn"),
                        new KeyValuePair<string, string>("GALX", galx),
                        new KeyValuePair<string, string>("gxf", gxf),
                        new KeyValuePair<string, string>("continue", cont),
                        new KeyValuePair<string, string>("ltmpl", "embedded"),
                        new KeyValuePair<string, string>("scc", "1"),
                        new KeyValuePair<string, string>("sarp", "1"),
                        new KeyValuePair<string, string>("oauth", "1"),
                        new KeyValuePair<string, string>("ProfileInformation", profileinformation),
                        new KeyValuePair<string, string>("_utf8", "?"),
                        new KeyValuePair<string, string>("bgresponse", "js_disabled"),
                        new KeyValuePair<string, string>("Email", username),
                        new KeyValuePair<string, string>("Passwd", password),
                        new KeyValuePair<string, string>("signIn", "Sign in"),
                        new KeyValuePair<string, string>("PersistentCookie", "yes"),
                    };
                    response = await client.PostAsync(third, new FormUrlEncodedContent(data2));
                    r = await response.Content.ReadAsStringAsync();
                    var clientid = "848232511240-73ri3t7plvk96pj4f85uj8otdat2alem.apps.googleusercontent.com";
                    var statewrapper_regex = "name=\"state_wrapper\" value=\"(.*?)\"";
                    var statewrapper = Regex.Matches(r, statewrapper_regex)[0].Groups[1].Value;
                    var connect_approve_regex = "id=\"connect-approve\" action=\"(.*?)\"";
                    var connect_approve = Regex.Matches(r, connect_approve_regex)[0].Groups[1].Value.Replace("&amp;", "&");
                    var data3 = new[]
                    {
                        new KeyValuePair<string, string>("submit_access", "true"),
                        new KeyValuePair<string, string>("state_wrapper", statewrapper),
                        new KeyValuePair<string, string>("_utf8", "?"),
                        new KeyValuePair<string, string>("bgresponse", "js_disabled"),
                    };
                    response = await client.PostAsync(connect_approve, new FormUrlEncodedContent(data3));
                    r = await response.Content.ReadAsStringAsync();
                    var code_regex = "id=\"code\" type=\"text\" readonly=\"readonly\" value=\"(.*?)\"";
                    var code = Regex.Matches(r, code_regex)[0].Groups[1].Value.Replace("&amp;", "&");
                    var data4 = new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientid),
                        new KeyValuePair<string, string>("client_secret", "NCjF1TLi2CcY6t5mt0ZveuL7"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", "urn:ietf:wg:oauth:2.0:oob"),
                        new KeyValuePair<string, string>("scope", "openid email https://www.googleapis.com/auth/userinfo.email"),
                    };
                    response = await client.PostAsync(last, new FormUrlEncodedContent(data4));
                    r = await response.Content.ReadAsStringAsync();
                    var jdata = JObject.Parse(r);
                    return jdata["id_token"].ToString();
                }
            }
        }

    }
}

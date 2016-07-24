using MandraSoft.PokemonGoApi.ConsoleTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoApi.ConsoleTest
{
    public class RadarCommunicator : IDisposable
    {
        private HttpClient _httpClient;
        public RadarCommunicator()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("http://localhost/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        public async Task<List<Pokemon>> GetUnknownPokemonsForArea(PokemonSpawnQuery query)
        {
            var res  = await _httpClient.PostAsJsonAsync("api/Pokemons/ListAll", query);
            return await res.Content.ReadAsAsync<List<Pokemon>>();
        }
    }

}

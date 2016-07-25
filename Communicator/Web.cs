using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.Helpers;
using MandraSoft.PokemonGo.Models;
using MandraSoft.PokemonGo.Models.PocoProtos;
using MandraSoft.PokemonGo.Models.WebModels.Requests;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using POGOProtos.Map;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Extensions.Compression.Client;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Communicator
{
    public class Web
    {
        private static object _lock = new object();
        private static Web _Instance;
        public static Web Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_Instance == null)
                        _Instance = new Web();
                    return _Instance;
                }
            }
        }
        private HttpClient _httpClient;        


        private Web()
        {
            InitHttpClient();
        }
        private void InitHttpClient()
        {
            _httpClient = new HttpClient(new ClientCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
            _httpClient.BaseAddress = new Uri(Configuration.WebUri);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public ConcurrentHashSet<ulong> _EncountersAlreadySent = new ConcurrentHashSet<ulong>();
        private IEnumerable<WildPokemonPoco> _Backup;
        private SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);
        private DateTime _lastSentData = DateTime.MinValue;
        private Dictionary<ulong, WildPokemonPoco> _Buffer = new Dictionary<ulong, WildPokemonPoco>();
        private object _BufferLock = new object();
        private int _errorCount = 0;
        /// <summary>
        /// WebSite Callback meant te be passed to PokemonGoClient.MapObjectsHandler
        /// Can be called by multiple threads and multiple clients.
        /// Will only send to webSite every 1 min.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task UpdateResponseToWebsite(PokemonGoClient client, GetMapObjectsResponse resp)
        {
            // Making sure it's a success before going on with the treatment.
            if (resp.Status != MapObjectsStatus.Success) return;
            // Converting POGProto to Poco ObjectList
            // Because when i tried to keep the state as POGProtos I kept having exception when adding/removing from RepeatedFields
            // Anyway all the serialization overhead was completely useless and a resource sink.
            var nCellList = new List<MapCellPoco>();
            foreach (var cell in resp.MapCells)
                nCellList.Add(new MapCellPoco(cell));
            foreach (var wild in nCellList.SelectMany(x => x.WildPokemons))
            {
                lock (_BufferLock)
                {
                    if (!_Buffer.ContainsKey(wild.EncounterId) && !_EncountersAlreadySent.Contains(wild.EncounterId))
                        _Buffer.Add(wild.EncounterId, wild);                    
                }
            }
            if (await _Semaphore.WaitAsync(1))
            {
                try
                {

                    if ((DateTime.UtcNow - _lastSentData).TotalSeconds > Configuration.WebDelay)
                    {
                        List<WildPokemonPoco> listToSend = new List<WildPokemonPoco>();
                        try
                        {

                            lock (_BufferLock)
                            {
                                listToSend = _Buffer.Values.ToList();
                                _Buffer = new Dictionary<ulong, WildPokemonPoco>();
                            }
                            if (_Backup.Any())
                            {
                                listToSend.AddRange(_Backup);
                            }
                            if (listToSend.Any())
                            {
                                await UpdateResponseToWebsite(listToSend);
                                _EncountersAlreadySent.AddRange(listToSend.Select(x => x.EncounterId));
                                _lastSentData = DateTime.UtcNow;
                                _Backup = new List<WildPokemonPoco>();
                            }
                            _errorCount = 0;
                        }
                        catch (Exception e)
                        {
                            _Backup = listToSend;
                            _errorCount++;
                            if (_errorCount >= 10)
                            {
                                Console.WriteLine("Error sending values to the website");
                                Console.WriteLine(e.Message);
                            }
                        }
                        
                    }
                }
                finally
                {
                    _Semaphore.Release();
                }
            }
        }
        public async Task<SpawnPointsImport> UpdateResponseToWebsite(IEnumerable<WildPokemonPoco> pokemons)
        {
            var importMsg = new SpawnPointsImport() { SpawnPoints = pokemons.GroupBy(x => x.SpawnPointId).Select(x => new PokemonGo.Models.WebModels.Mixed.SpawnPoint() { Id = x.Key, Latitude = x.First().Latitude, Longitude = x.First().Longitude, Encounters = x.Select(a => new PokemonGo.Models.WebModels.Mixed.Encounter() { Id = a.EncounterId, PokemonId = (int)a.PokemonData.PokemonId, SpawnTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(a.LastModifiedTimestampMs).AddMilliseconds(a.TimeTillHiddenMs).AddMinutes(-15) }).ToList() }).ToList() };
            var t = importMsg.SpawnPoints.SelectMany(e => e.Encounters).GroupBy(e => e.Id).OrderByDescending(x => x.Count()).ToList();
            var servResponse = await _httpClient.PostAsJsonAsync("api/SpawnPoints/PostEncounter", importMsg);
            var result = await servResponse.Content.ReadAsAsync<BaseResponse>();
            Console.WriteLine(result.Message);
            if (!result.Success)
            {
                throw new Exception(result.Message);
            }
            return importMsg;
        }
    }
}

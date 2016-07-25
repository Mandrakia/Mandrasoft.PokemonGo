using Google.Common.Geometry;
using MandraSoft.PokemonGo.Models;
using MandraSoft.PokemonGo.Models.Enums;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using MandraSoft.PokemonGo.Api.Extensions;
using MandraSoft.PokemonGo.Api.Helpers;
using MandraSoft.PokemonGo.Api.LoginProviders;
using MandraSoft.PokemonGo.Api.Managers;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Extensions.Compression.Client;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api
{
    public class PokemonGoClient : IDisposable
    {
        public delegate Task MapObjectsUpdate(PokemonGoClient client, GetMapObjectsResponse response);

        /// <summary>
        /// It's a callback function called on every GetMapObjectsResponse received. It can be null, it's only useful if you want to export Data elsewhere (ie : Database or WebsiteAPI etc).
        /// </summary>
        public MapObjectsUpdate MapObjectsHandler { get; set; }
        public delegate Task WalkCallback(PokemonGoClient client);
        public string Name { get; set; }

        public string LoginUser { get; set; } = Configuration.Login;
        public string Password { get; set; } = Configuration.Password;

        internal HttpClient _httpClient;
        internal string _token,_apiUrl;
        private AuthType _provider;
        private double _lat, _lng,_alt;
        public double Latitude => _lat;
        public double Longitude => _lng;
        public double Altitude => _alt;
        internal S2LatLng Location
        {
            get { return S2LatLng.FromDegrees(Latitude, Longitude); }
        }

        internal AuthTicket _authTicket;
        public MapsCellsManager MapManager { get; set; }
        public InventoryManager InventoryManager { get; set; }

        public PokemonGoClient(string token, AuthType type) : this()
        {
            _token = token;
            _provider = type;
        }
        public PokemonGoClient(double lat, double lng) : this()
        {
            _lat = lat;
            _lng = lng;
            _alt = 10;
        }
        public PokemonGoClient()
        {
             InitManagers();
            _httpClient = GetHttpClient();
        }
        public HttpClient GetHttpClient()
        {
            ClientCompressionHandler handler = new ClientCompressionHandler(new GZipCompressor(), new DeflateCompressor());
            var _httpClient = new HttpClient(new RetryHandler(handler));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            //"Dalvik/2.1.0 (Linux; U; Android 5.1.1; SM-G900F Build/LMY48G)");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                "application/x-www-form-urlencoded");
            _httpClient.Timeout = new TimeSpan(0, 0, 10);
            return _httpClient;
        }
        public void InitManagers()
        {
            InventoryManager = new InventoryManager();
            MapManager = new MapsCellsManager();
        }

        public void SetCoordinates(double lat, double lng)
        {
            _lat = lat;
            _lng = lng;
        }
        public async Task Login()
        {
            if (Configuration.AuthType == AuthType.PTC)
            {
                _provider = AuthType.PTC;
                _token = await PtcLogin.GetAccessToken(LoginUser,Password);
            }
            else if (Configuration.AuthType == AuthType.Google)
            {
                _provider = AuthType.Google;
                _token = await GoogleLogin.LoginGoogle(LoginUser,Password);
            }
        }
        public async Task LoginPtc(string login,string password)
        {
            LoginUser = login;
            Password = password;
            await LoginPtc();
        }
        public async Task LoginPtc()
        {
            _token = await PtcLogin.GetAccessToken(LoginUser,Password);
            _provider = AuthType.PTC;
        }
        public async Task LoginGoogle()
        {
            _token = await GoogleLogin.LoginGoogle(LoginUser,Password);
            _provider = AuthType.Google;
        }
        public async Task LoginGoogle(string username, string password)
        {
            LoginUser = username;
            Password = password;
            await LoginGoogle();
        }

        public TimeSpan GetWalkingDuration(double lat, double lng, TravelingSpeed speed = TravelingSpeed.Walk)
        {
            var destination = S2LatLng.FromDegrees(lat, lng);
            var distance = Location.GetEarthDistance(destination);
            double speedF = 0;
            switch (speed)
            {
                case TravelingSpeed.Walk:
                    speedF = Globals.WalkingSpeed;
                    break;
                case TravelingSpeed.Bicycle:
                    speedF = Globals.BicycleSpeed;
                    break;
                case TravelingSpeed.Car:
                    speedF = Globals.CarSpeed;
                    break;
            }
            return TimeSpan.FromSeconds(distance / speedF);
        }
        public async Task WalkTo(double lat, double lng,TravelingSpeed speed = TravelingSpeed.Walk, WalkCallback callback = null)
        {
            Console.WriteLine($"Walking toward : {lat} {lng} , ETA : {GetWalkingDuration(lat, lng)}");
            var destination = S2LatLng.FromDegrees(lat, lng);
            var heading = S2Helper.ComputeHeading(Location, destination);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            double speedF = 0;
            switch (speed)
            {
                case TravelingSpeed.Walk:
                    speedF = Globals.WalkingSpeed;
                    break;
                case TravelingSpeed.Bicycle:
                    speedF = Globals.BicycleSpeed;
                    break;
                case TravelingSpeed.Car:
                    speedF = Globals.CarSpeed;
                    break;
            }
            while (Location.GetEarthDistance(destination) > Globals.AcceptedRadius)
            {
                //3 seconds refresh rate.
                var distToTravel = Math.Min(speedF * 3, Location.GetEarthDistance(destination));
                var nDestination = S2Helper.ComputeOffset(Location, distToTravel, heading);
                await this.GetPlayerUpdateResponse(nDestination.LatDegrees, nDestination.LngDegrees);
                sw.Restart();
                if (callback != null)
                    await callback(this);
                var elapsed = sw.ElapsedMilliseconds;
                if(elapsed < 3000)
                    await Task.Delay(3000 - (int)elapsed);
            }
        }

        public async Task SetServer()
        {
            var serverResponse = await _httpClient.GetEnvelope(this, false, Globals.RpcUrl,
                this.GetPlayerRequest(),
                this.GetHatchedEggRequest(),
                this.GetInventoryRequest(),
                this.GetCheckAwardedBadgeRequest(),
                this.GetDownloadSettingsRequest());

            _authTicket = new AuthTicket()
            {
                End = serverResponse.AuthTicket.End,
                ExpireTimestampMs = serverResponse.AuthTicket.ExpireTimestampMs,
                Start = serverResponse.AuthTicket.Start
            };
            _apiUrl = $"https://{serverResponse.ApiUrl}/rpc";
        }       

        public async Task<POGOProtos.Networking.Envelopes.RequestEnvelope> GetRequest(bool withAuthTicket = true,params Request[] customRequests)
        {
            var request = new POGOProtos.Networking.Envelopes.RequestEnvelope()
            {
                Altitude = _alt,                
                Latitude = _lat,
                Longitude = _lng,
                RequestId = 1469378659230941192,
                StatusCode = 2,
                Unknown12 = 989, //Required otherwise we receive incompatible protocol
                Requests =
                {
                    customRequests
                }
            };
            if (withAuthTicket)
            {
                request.AuthTicket = _authTicket;
            }
            else request.AuthInfo = new RequestEnvelope.Types.AuthInfo() { Provider = _provider == AuthType.Google ? "google" : "ptc", Token = new RequestEnvelope.Types.AuthInfo.Types.JWT() { Contents = _token, Unknown2 = 14 } };
            return request;
        }

        public void Dispose()
        {
            if (MapManager != null)
            {
                MapManager.Dispose();
                MapManager = null;
            }
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }
    }
}

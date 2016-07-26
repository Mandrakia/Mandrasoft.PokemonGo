using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.Managers;
using MandraSoft.PokemonGo.DAL;
using MandraSoft.PokemonGo.Models.WebModels.Mixed;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using MandraSoft.PokemonGo.Web.Jobs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected string _LiveStateFile;
        protected string _stateFile;
        protected void Application_Start()
        {
            _stateFile = Server.MapPath("~/SaveState.json");
            _LiveStateFile = Server.MapPath("~/LiveState.json");
            SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            if (File.Exists(_stateFile))
            {
                Globals.DumpQueue = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SpawnPoint>>(System.IO.File.ReadAllText(_stateFile));
                File.Delete(_stateFile);
            }
            if (File.Exists(_LiveStateFile))
            {
                Globals.LivePokemons = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<long, MapPokemon>>>(System.IO.File.ReadAllText(_LiveStateFile));
                File.Delete(_LiveStateFile);
            }
            using (var db = new PokemonDb())
            {
                var pokemons = db.Pokemons.ToList();
                Globals.PokemonNamesById = pokemons.ToDictionary(x => x.Id, x => new Dictionary<string, string>() { { "fr", x.LabelFr }, { "en", x.LabelEn }, { "de", x.LabelDe } });
                Globals.PokemonNamesByLang = new Dictionary<string, Dictionary<int, string>>()
                {
                    {"fr", pokemons.ToDictionary(x=> x.Id, x=> x.LabelFr) },
                    {"de", pokemons.ToDictionary(x=> x.Id, x=> x.LabelDe) },
                    {"en", pokemons.ToDictionary(x=> x.Id, x=> x.LabelEn) }
                };
            }
            //var MapManager = new MapsCellsManager();
            //if (HttpRuntime.Cache["Client"] == null)
            //{
            //    var cli = new PokemonGoClient();
            //    cli.MapObjectsHandler = Communicator.Db.UpdateResponseToDb;
            //    cli.Name = "ClientParis";
            //    cli.MapManager = MapManager;
            //    cli.LoginPtc().Wait();
            //    cli.SetServer().Wait();
            //    HttpRuntime.Cache.Add("Client", cli, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            //}
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
        protected void Application_End()
        {
            var backup = new List<SpawnPoint>();
            if (ScanningJobs._Backup != null)
                backup.AddRange(ScanningJobs._Backup);
            lock(Globals.DumpQueueLock)
                backup.AddRange(Globals.DumpQueue);
           
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(backup);
            System.IO.File.WriteAllText(_stateFile, json);

            json = Newtonsoft.Json.JsonConvert.SerializeObject(Globals.LivePokemons);
            System.IO.File.WriteAllText(_LiveStateFile, json);
        }
    }
}

using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.Managers;
using MandraSoft.PokemonGo.Models.WebModels.Mixed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace MandraSoft.PokemonGo.Web
{
    static public class Globals
    {
        static public object DumpQueueLock = new object();
        static public List<SpawnPoint> DumpQueue = new List<SpawnPoint>();
        static private object _lock = new object();
        static public MapsCellsManager MapManager { get; set; }
        static public Dictionary<int,Dictionary<string, string>> PokemonNamesById;
        static public Dictionary<string, Dictionary<int, string>> PokemonNamesByLang;
    }
}
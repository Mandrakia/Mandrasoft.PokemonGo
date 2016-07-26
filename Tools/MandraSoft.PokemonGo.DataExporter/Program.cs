using MandraSoft.PokemonGo.Models.WebModels.Mixed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.DataExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new PokemonGo.DAL.PokemonDb())
            {
                ctx.Configuration.ProxyCreationEnabled = false;
                var jsonString = string.Empty;
                //var spawns = ctx.SpawnPoints.ToList().Select(x=> new SpawnPoint() { Latitude = x.Location.Latitude.Value, Longitude = x.Location.Longitude.Value, Id = x.Id, Encounters = new List<Encounter>() }).ToList();
                //var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(spawns);
                //System.IO.File.WriteAllText(@"e:\RawDumps\SpawnPoints.json", jsonString);
                //spawns = null;
                //jsonString = null;
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                for (var i = 4000000; i < ctx.Encounters.Count(); i += 1000000)
                {
                    var encounters = ctx.Encounters.OrderBy(x=> x.Id).Skip(i).Take(1000000).ToList().Select(x => new Encounter() { Id = x.IdU, PokemonId = x.PokemonId, SpawnTime = x.EstimatedSpawnTime, SpawnId = x.SpawnPointId }).ToList();
                    jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(encounters);
                    System.IO.File.WriteAllText(@"e:\RawDumps\Encounters-"+ i.ToString() + ".json", jsonString);
                    jsonString = null;
                    encounters = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}

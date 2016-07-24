using System.Collections.Generic;
using MandraSoft.PokemonGo.Models.WebModels.Mixed;

namespace MandraSoft.PokemonGo.Models.WebModels.Requests
{
    public class SpawnPointsImport
    {
        public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
    }
}

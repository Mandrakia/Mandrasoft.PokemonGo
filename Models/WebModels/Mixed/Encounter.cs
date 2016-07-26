using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WebModels.Mixed
{
    public class Encounter
    {
        public ulong Id { get; set; }
        public int PokemonId { get; set; }
        public string PokemonName { get; set; }
        public DateTime SpawnTime { get; set; }
        public string SpawnId { get; set; }
    }
}

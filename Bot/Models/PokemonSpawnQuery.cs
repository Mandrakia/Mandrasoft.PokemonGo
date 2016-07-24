using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoApi.ConsoleTest.Models
{
    public class PokemonSpawnQuery
    {
        public double swLat { get; set; }
        public double swLng { get; set; }
        public double neLat { get; set; }
        public double neLng { get; set; }
        public List<int> pokemonIds { get; set; }
        public PokemonSpawnQuery()
        {
            pokemonIds = new List<int>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WebModels.Requests
{   
    public class MapQuery : BaseRequest
    {
        public double swLat { get; set; }
        public double swLng { get; set; }
        public double neLat { get; set; }
        public double neLng { get; set; }
        public List<int> pokemonIds { get; set; } = new List<int>();        
    }
}

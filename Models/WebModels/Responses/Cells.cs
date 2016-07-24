using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WebModels.Responses
{
    public class LatLng
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class CellResponse
    {
        public List<LatLng> Centers { get; set; }
        public List<Cell> Cells { get; set; }
    }
    public class Cell
    {
        public List<LatLng> Shape { get; set; }
        public bool Center { get; set; }
        public ulong Id { get; set; }
    }
}

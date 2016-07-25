using Google.Common.Geometry;
using MandraSoft.PokemonGo.Api.Helpers;
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
        public List<LatLng> Centers { get; set; } = new List<LatLng>();
        public List<Cell> Cells { get; set; } = new List<Cell>();
    }
    public class Cell
    {
        public List<LatLng> Shape { get; set; }
        public bool Center { get; set; }
        public string Id { get; set; }

        public Cell()
        { }
        public Cell(S2CellId cell)
        {
            Id = cell.Id.ToString();
            Shape = cell.GetShape().Select(x => new LatLng() { lat = x.LatDegrees, lng = x.LngDegrees }).ToList();
        }
    }
}

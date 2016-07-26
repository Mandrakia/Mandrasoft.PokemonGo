using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WPFViewModels
{
    public class LatLngViewModel : BaseViewModel
    {
        private double _Latitude;
        public double Latitude
        {
            get { return _Latitude; }
            set { if (_Latitude != value) { _Latitude = value; OnPropertyChanged(); } }
        }

        private double _Longitude;
        public double Longitude
        {
            get { return _Longitude; }
            set { if (_Longitude != value) { _Longitude = value; OnPropertyChanged(); } }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WPFViewModels
{
    public class ScannedAreaViewModel : BaseViewModel
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { if (_Name != value) { _Name = value; OnPropertyChanged(); } }
        }

        private int _HexNum = 20;
        public int HexNum
        {
            get { return _HexNum; }
            set { if (_HexNum != value) { _HexNum = value; OnPropertyChanged(); } }
        }
        private ObservableCollection<String> _AccountNames = new ObservableCollection<string>();
        public ObservableCollection<String> AccountNames
        {
            get { return _AccountNames; }
            set { if (_AccountNames != value) { _AccountNames = value; OnPropertyChanged(); } }
        }
        [JsonIgnore]
        public ObservableCollection<AccountsViewModel> Accounts
        {
            get { return new ObservableCollection<AccountsViewModel>(Configuration.Settings.Accounts.Where(x => AccountNames.Contains(x.Name)).ToList()); }
  
        }
        [JsonIgnore]
        public ObservableCollection<string> AllAvailableAccountNames
        {
            get { return new ObservableCollection<string>(Configuration.Settings.Accounts.Select(x=> x.Name).ToList()); }

        }

        private int _JobsCount = 2;
        public int JobsCount
        {
            get { return _JobsCount; }
            set { if (_JobsCount != value) { _JobsCount = value; OnPropertyChanged(); } }
        }

        private LatLngViewModel _Coordinates = new LatLngViewModel();
        public LatLngViewModel Coordinates
        {
            get { return _Coordinates; }
            set { if (_Coordinates != value) { _Coordinates = value; OnPropertyChanged(); } }
        }

        private string _Warning;
        public string Warning
        {
            get { return _Warning; }
            set { if (_Warning != value) { _Warning = value; OnPropertyChanged(); } }
        }
    }
}

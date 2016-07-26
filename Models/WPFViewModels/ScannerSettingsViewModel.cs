using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WPFViewModels
{
    public class ScannerSettingsViewModel : BaseViewModel
    {
        private int _WebDelay = 30;
        public int WebDelay
        {
            get { return _WebDelay; }
            set { if (_WebDelay != value) { _WebDelay = value; OnPropertyChanged(); } }
        }
        private ObservableCollection<ScannedAreaViewModel> _ScannedAreas;
        public ObservableCollection<ScannedAreaViewModel> ScannedAreas
        {
            get { return _ScannedAreas; }
            set { if (_ScannedAreas != value) { _ScannedAreas = value; OnPropertyChanged(); } }
        }
        private ObservableCollection<AccountsViewModel> _Accounts = new ObservableCollection<AccountsViewModel>();
        public ObservableCollection<AccountsViewModel> Accounts
        {
            get { return _Accounts; }
            set { if (_Accounts != value) { _Accounts = value; OnPropertyChanged(); } }
        }
        private bool _SaveLocalDb { get; set; }
        public bool SaveLocalDb
        {
            get { return _SaveLocalDb; }
            set { if (_SaveLocalDb != value) { _SaveLocalDb = value; OnPropertyChanged(); } }
        }
        private string _WebUri = "http://pokemongo.mandrasoft.fr";
        public string WebUri
        {
            get { return _WebUri; }
            set { if (_WebUri != value) { _WebUri = value; OnPropertyChanged(); } }
        }
        private int _DbDelay { get; set; } = 60;
        public int DbDelay
        {
            get { return _DbDelay; }
            set { if (_DbDelay != value) { _DbDelay = value; OnPropertyChanged(); } }
        }
    }
}

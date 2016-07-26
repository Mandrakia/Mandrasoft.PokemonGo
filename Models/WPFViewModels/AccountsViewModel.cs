using MandraSoft.PokemonGo.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WPFViewModels
{
    public class AccountsViewModel : BaseViewModel
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { if (_Name != value) { _Name = value; OnPropertyChanged(); } }
        }


        private string _Login;
        public string Login
        {
            get { return _Login; }
            set { if (_Login != value) { _Login = value; OnPropertyChanged(); } }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set { if (_Password != value) { _Password = value; OnPropertyChanged(); } }
        }
        private AuthType _AuthType;
        public AuthType AuthType
        {
            get { return _AuthType; }
            set { if (_AuthType != value) { _AuthType = value; OnPropertyChanged(); } }
        }
    }
}

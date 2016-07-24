using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WebModels.Requests
{
    public class LoginContainer : BaseRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}

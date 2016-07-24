using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.WebModels.Requests
{
    abstract public class BaseRequest
    {
        public string lang { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MandraSoft.PokemonGo.Models.WebModels.Responses
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
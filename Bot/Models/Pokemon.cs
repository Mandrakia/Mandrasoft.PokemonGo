using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace MandraSoft.PokemonGoApi.ConsoleTest.Models
{
    public class Pokemon
    {
        public double? Latitude  {get;set;}
        public double? Longitude { get; set; }
        public int PokedexNumber { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Name { get; set; }
        public string SpawnPointId { get; set; }
        public ulong EncounterId { get; set; }
    }
}
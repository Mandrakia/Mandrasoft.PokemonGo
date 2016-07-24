using System;

namespace MandraSoft.PokemonGo.Models.WebModels.Responses
{
    public class MapPokemon
    {
        public long EncounterId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int PokedexNumber { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Name { get; set; }
    }
}
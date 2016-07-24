using POGOProtos.Data;
using POGOProtos.Enums;

namespace MandraSoft.PokemonGo.Models.PocoProtos
{
    public class PokemonDataPoco
    {
        public PokemonId PokemonId { get; set; }

        public PokemonDataPoco() { }
        public PokemonDataPoco(PokemonData poke)
        {
            PokemonId = poke.PokemonId;
        }
    }
}
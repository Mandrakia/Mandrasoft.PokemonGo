using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;

namespace MandraSoft.PokemonGo.Models.PocoProtos
{
    public class MapPokemonPoco
    {
        public ulong EncounterId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string SpawnPointId { get; set; }
        public PokemonId PokemonId { get; set; }
        public long ExpirationTimestampMs { get; set; }
        public MapPokemonPoco(MapPokemon poke)
        {
            EncounterId = poke.EncounterId;
            Latitude = poke.Latitude;
            Longitude = poke.Longitude;
            SpawnPointId = poke.SpawnPointId;
            PokemonId = poke.PokemonId;
            ExpirationTimestampMs = poke.ExpirationTimestampMs;
        }
    }
}
using POGOProtos.Map.Pokemon;

namespace MandraSoft.PokemonGo.Models.PocoProtos
{
    public class WildPokemonPoco
    {
        public ulong EncounterId { get; set; }
        public long LastModifiedTimestampMs { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TimeTillHiddenMs { get; set; }
        public string SpawnPointId { get; set; }
        public PokemonDataPoco PokemonData { get; set; }
        public WildPokemonPoco() { }
        public WildPokemonPoco(WildPokemon poke)
        {
            EncounterId = poke.EncounterId;
            LastModifiedTimestampMs = poke.LastModifiedTimestampMs;
            Latitude = poke.Latitude;
            Longitude = poke.Longitude;
            TimeTillHiddenMs = poke.TimeTillHiddenMs;
            SpawnPointId = poke.SpawnPointId;
            PokemonData = new PokemonDataPoco(poke.PokemonData);
        }
    }
}
using POGOProtos.Map.Fort;

namespace MandraSoft.PokemonGo.Models.PocoProtos
{
    public class FortDataPoco
    {
        public string Id { get; set; }
        public long CooldownCompleteTimestampMs { get; set; }
        public bool Enabled { get; set; }
        public FortType Type { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long LastModifiedTimestampMs { get; set; }
        public FortDataPoco(FortData fort)
        {
            Id = fort.Id;
            CooldownCompleteTimestampMs = fort.CooldownCompleteTimestampMs;
            Enabled = fort.Enabled;
            Type = fort.Type;
            Latitude = fort.Latitude;
            Longitude = fort.Longitude;
            LastModifiedTimestampMs = fort.LastModifiedTimestampMs;
        }
    }
}
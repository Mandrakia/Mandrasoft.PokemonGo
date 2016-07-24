using POGOProtos.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.PocoProtos
{
    public class MapCellPoco
    {
        public ulong S2CellId { get; set; }
        public long CurrentTimestampMs { get; set; }
        public List<FortDataPoco> Forts { get; set; }
        public List<SpawnPointPoco> SpawnPoints { get; set; }
        public List<string> DeletedObjects { get; set; }
        public bool IsTruncatedList { get; set; }
        public List<FortSummaryPoco> FortSummaries { get; set; }
        public List<SpawnPointPoco> DecimatedSpawnPoints { get; set; }
        public List<WildPokemonPoco> WildPokemons { get; set; }
        public List<MapPokemonPoco> CatchablePokemons { get; set; }
        public List<NearbyPokemonPoco> NearbyPokemons { get; set; }

        public MapCellPoco(MapCell cell)
        {
            S2CellId = cell.S2CellId;
            CurrentTimestampMs = cell.CurrentTimestampMs;
            Forts = new List<FortDataPoco>();
            if (cell.Forts != null)
                cell.Forts.ToList().ForEach(x => Forts.Add(new FortDataPoco(x)));
            SpawnPoints = new List<SpawnPointPoco>();
            if (cell.SpawnPoints != null)
                cell.SpawnPoints.ToList().ForEach(x => SpawnPoints.Add(new SpawnPointPoco(x)));
            DeletedObjects = new List<string>();
            if (cell.DeletedObjects != null)
                cell.DeletedObjects.ToList().ForEach(x => DeletedObjects.Add(x));
            IsTruncatedList = cell.IsTruncatedList;
            FortSummaries = new List<FortSummaryPoco>();
            if (cell.FortSummaries != null)
                cell.FortSummaries.ToList().ForEach(x => FortSummaries.Add(new FortSummaryPoco(x)));
            DecimatedSpawnPoints = new List<SpawnPointPoco>();
            if (cell.DecimatedSpawnPoints != null)
                cell.DecimatedSpawnPoints.ToList().ForEach(x => DecimatedSpawnPoints.Add(new SpawnPointPoco(x)));
            WildPokemons = new List<WildPokemonPoco>();
            if (cell.WildPokemons != null)
                cell.WildPokemons.ToList().ForEach(x => WildPokemons.Add(new WildPokemonPoco(x)));
            CatchablePokemons = new List<MapPokemonPoco>();
            if (cell.CatchablePokemons != null)
                cell.CatchablePokemons.ToList().ForEach(x => CatchablePokemons.Add(new MapPokemonPoco(x)));
            NearbyPokemons = new List<NearbyPokemonPoco>();
            if (cell.NearbyPokemons != null)
                cell.NearbyPokemons.ToList().ForEach(x => NearbyPokemons.Add(new NearbyPokemonPoco(x)));
        }
    }
}

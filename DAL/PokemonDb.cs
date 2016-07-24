using MandraSoft.PokemonGo.Models.Entities;
using System.Data.Entity;

namespace MandraSoft.PokemonGo.DAL
{
    public class PokemonDb : DbContext
    {
        /// <summary>
        /// Mainly used when transfering from One Db To the other.
        /// to avoid having schema change errors.
        /// </summary>
        /// <param name="connString"></param>
        public PokemonDb(string connString) : base(connString)
        {
            Database.SetInitializer<PokemonDb>(null); //removes the boring exception on model change.
        }
        public PokemonDb() : base("PokemonGoDb")
        { }
        public DbSet<SpawnPoint> SpawnPoints { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<Pokemon> Pokemons { get; set; }
    }
}

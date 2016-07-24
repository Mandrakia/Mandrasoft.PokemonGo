using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoMapImporter
{
    
    [Table("pokemon")]
    class Pokemon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("encounter_id")]
        public string EncounterId { get; set; }
        [Column("spawnpoint_id")]
        public string SpawnPointId { get; set; }
        [Column("pokemon_id")]
        public int PokemonId { get; set; }
        [Column("latitude")]
        public double Latitude { get; set; }
        [Column("longitude")]
        public double Longitude { get; set; }
        [Column("disappear_time")]
        public DateTime Disappear_Time { get; set; }
    }
    class PokemonGoMapContext : DbContext
    {
        public PokemonGoMapContext(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        { }
        public PokemonGoMapContext(string connString) : base(connString) { }
        public DbSet<Pokemon> Pokemons { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.Entities
{
    public class SpawnPoint
    {
        public string Id { get; set; }
        public DbGeography Location { get; set; }
        public virtual ICollection<Encounter> Encounters { get; set; }
    }
}

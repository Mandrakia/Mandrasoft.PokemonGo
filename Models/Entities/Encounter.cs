using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.Entities
{
  
    public class Encounter
    {
        [NotMapped]
        public long __Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get { return __Id; } set { __Id = value; } }
        public ulong IdU
        {
            get
            {
                unchecked
                {
                    return (ulong)__Id;
                }
            }

            set
            {
                unchecked
                {
                    __Id = (long)value;
                }
            }
        }
        public int PokemonId { get; set; }
        public virtual Pokemon Pokemon { get; set; }
        public string SpawnPointId { get; set; }
        public virtual SpawnPoint SpawnPoint { get; set; }
        public DateTime EstimatedSpawnTime { get; set; }
        [Index]
        public DateTime ExpirationTime { get; set; }
    }
}

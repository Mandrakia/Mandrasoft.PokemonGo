using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Models.Entities
{
    public class Pokemon
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Index]
        [MaxLength(50)]
        public string LabelEn { get; set; }
        [Index]
        [MaxLength(50)]
        public string LabelFr { get; set; }
        [Index]
        [MaxLength(50)]
        public string LabelDe { get; set; }
    }
}

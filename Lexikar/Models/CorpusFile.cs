using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lexikar.Models
{
    public class CorpusFile // : BaseEntity
    {
        [Key]
        public int ID { get; set; } // Inherit from some base entity
        public string Name { get; set; }
        public virtual ICollection<CorpusSource> CorpusSources { get; set; }
        public int CloneID { get; set; } // Use new table for mapping
    }
}

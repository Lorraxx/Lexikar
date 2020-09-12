using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lexikar.Models
{
    public class CorpusSource // : BaseEntity
    {
        [Key]
        public int ID { get; set; } // Inherit from some base entity
        public string TextRow { get; set; }
        public int RowNumber { get; set; }
        [ForeignKey("CorpusFile")]
        public int FileSourceID { get; set; }
        public virtual CorpusFile CorpusFile { get; set; }
        public int CloneID { get; set; } // Use new table for mapping
    }
}

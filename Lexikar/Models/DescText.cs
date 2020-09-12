using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lexikar.Models
{
    public class DescText // : BaseEntity
    {
        [Key]
        public int ID { get; set; } // Inherit from some base entity
        public string text { get; set; }
        public string what { get; set; } // Use enmuration table instead 
        public int CloneID { get; set; } // Use new table for mapping
    }
}

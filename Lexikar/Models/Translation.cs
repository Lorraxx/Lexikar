using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Lexikar.Models
{
    public class Translation // : BaseEntity
    {
        [Key]
        public int ID { get; set; } // Inherit from some base entity
        public bool Nom { get; set; } // Full property names not abbreviations
        public bool Verb { get; set; }// Use english property names
        public bool Ost { get; set; }
        public string FR { get; set; } // Wrong property names
        public string CS { get; set; } // other languages are used as well
        public string Zdroj { get; set; } 
        public string Systematika { get; set; } 
        public string Nezamnenovat { get; set; }
        public string Poznamka { get; set; }
        public int CloneID { get; set; } // Use new table for mapping
    }
}

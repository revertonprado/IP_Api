using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Domain.Entities
{
    public class IPAddresses
    {
        [Key]
        public int Id { get; set;}
        public int CountryId { get; set; }
        public string IP { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Countries Countries { get; set; }

    }
}

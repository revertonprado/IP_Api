using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IP_Domain.ViewModel
{
    public class IpAdressXCountriesViewModel
    {
        public string IP { get; set; }
        public string CountryName { get; set; }
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
    }

    public class IpAdressXCountriesIDViewModel
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string CountryName { get; set; }
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [Keyless]
    public class IpCountryReportViewModel
    {
        public string CountryName { get; set; }
        public int AdressessCount { get; set; }
        public DateTime LastTimeUpdated { get; set; }
    }


}

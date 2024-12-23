using IP_Domain.Entities;
using IP_Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IP_Domain.Interfaces
{
    public interface IIPAdressRepository
    {
        IpAdressXCountriesViewModel GetIPAddress(string ipAddresses);

        IpAdressXCountriesViewModel GetIPAddressID(int id);
        Task AddIPAddressAsync(IpAdressXCountriesViewModel ipAdressXcountries);
        Task<List<IpAdressXCountriesIDViewModel>> GetIpAddressesUpdateAsync(int limit);
        Task PutIpAddressesUpdateAsync(IpAdressXCountriesIDViewModel ipAdressXCountriesIDViewModel);
        List<IpCountryReportViewModel> GetIpCountryReport(string iplist);
        

    }
}

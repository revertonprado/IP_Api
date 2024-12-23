using IP_Domain.Entities;
using IP_Domain.Interfaces;
using IP_Domain.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using IP_Domain.Services;
using System.Net;
using System.Collections.Generic;

namespace IP_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpController : ControllerBase
    {
        private readonly IIPAdressRepository _ipAdressRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;

        private const string KEY_IPADRESS = "IpAdress";

        public IpController(IIPAdressRepository ipAdressRepository, IMemoryCache memoryCache, HttpClient httpClient)
        {
            _ipAdressRepository = ipAdressRepository;
            _memoryCache = memoryCache;
            _httpClient = httpClient;
        }

        [HttpGet("GetIpAdress/{ip}")]
        public async Task<IActionResult> GetIpAdress(string ip)
        {
            bool isValid = IpValidator.IsValidIp(ip);

            if (isValid)
            {
                if (_memoryCache.TryGetValue(KEY_IPADRESS, out List<IpAdressXCountriesViewModel> ipadressCache))
                {
                    var cachedIp = ipadressCache.FirstOrDefault(x => x.IP == ip);
                    if (cachedIp != null)
                    {
                        return Ok(cachedIp);
                    }
                }

                var ipAddress = _ipAdressRepository.GetIPAddress(ip);

                if (ipAddress != null)
                {
                    if (ipadressCache == null)
                    {
                        ipadressCache = new List<IpAdressXCountriesViewModel>();
                    }

                    ipadressCache.Add(ipAddress);

                    _memoryCache.Set(KEY_IPADRESS, ipadressCache, TimeSpan.FromHours(1));
                    return Ok(ipAddress);
                }

                var resultadd = await AddIpAdress(ip);

                if (resultadd is OkObjectResult okResult)
                {
                    var resultValue = okResult.Value; 
                    return Ok(resultValue);  
                }

                return BadRequest($"Try Again Later");
            }
            else
            {
                return BadRequest($"The IP address {ip} is not valid.");
            }
        }

        [HttpGet("GetIpAdressReportr")]
        public IActionResult GetIpAdressReport([FromQuery] List<string> listip)
        {
       
            var stringlist = string.Join(", ", listip);

            var resultadd =  _ipAdressRepository.GetIpCountryReport(stringlist);

            return Ok(new { resultadd });

        }

        private async Task<IActionResult> AddIpAdress(string ip)
        {
            List<IpAdressXCountriesViewModel> ipadressCache = new List<IpAdressXCountriesViewModel>();

            var url = $"http://ip2c.org/{ip}";

            var result = await _httpClient.GetAsync(url);

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();

                switch (content[0])
                {
                    case '0':
                        return BadRequest("Something went wrong with the external API");

                    case '1':
                        var reply = content.Split(';');
                        var countryInfo = new IpAdressXCountriesViewModel
                        {
                            IP = ip,
                            TwoLetterCode = reply[1],
                            ThreeLetterCode = reply[2],
                            CountryName = reply[3]
                        };

                        await _ipAdressRepository.AddIPAddressAsync(countryInfo);

                        if (ipadressCache == null)
                        {
                            ipadressCache = new List<IpAdressXCountriesViewModel>();
                        }

                        ipadressCache.Add(countryInfo);
                        _memoryCache.Set(KEY_IPADRESS, ipadressCache, TimeSpan.FromHours(1));

                        return Ok(countryInfo);

                    default:
                        return BadRequest("Unknown response format");
                }
            }
            else
            {
                return NotFound();
            }
        }
    }

    public class IpValidator
    {
        public static bool IsValidIp(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }
    }
}

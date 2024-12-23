using Hangfire;
using IP_Domain.Interfaces;
using IP_Domain.ViewModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Domain.Services
{
    public class SqlJobsService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly IIPAdressRepository _adressRepository;
        private readonly ILogger<SqlJobsService> _logger;

        private const string KEY_IPADRESS = "IpAdress";

        private static bool _isJobRunning = false;

        public SqlJobsService(IMemoryCache memoryCache, HttpClient httpClient, IIPAdressRepository adressRepository, ILogger<SqlJobsService> logger)
        {
            _memoryCache = memoryCache;
            _httpClient = httpClient;
            _adressRepository = adressRepository;
            _logger = logger;
        }

        public async Task UpdateIPJob()
        {
            if (_isJobRunning)
            {
                _logger.LogInformation("Job already in execution. Ignoring duplicated call.");
                return;
            }


            _logger.LogInformation("Initializing job Update IPs.");

            int batchSize = 100;
            int batchCount = 0;

            try
            {

                while (true)
                {
                    var existingRecords = await _adressRepository.GetIpAddressesUpdateAsync(100);

                    if (existingRecords == null || !existingRecords.Any())
                    {
                        _logger.LogInformation("No registers found. Finishing job.");
                        break;
                    }

                    foreach (var existingRecord in existingRecords)
                    {
                        var ip = await GetIpAdressReq(existingRecord.IP);

                        var updatedInfo = new IpAdressXCountriesIDViewModel
                        {
                            Id = existingRecord.Id,
                            IP = existingRecord.IP,
                            TwoLetterCode = ip.TwoLetterCode,
                            ThreeLetterCode = ip.ThreeLetterCode,
                            CountryName = ip.CountryName,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _adressRepository.PutIpAddressesUpdateAsync(updatedInfo);
                        _logger.LogInformation($"IP {existingRecord.IP} updated successfully.");

                        if (_memoryCache.TryGetValue(KEY_IPADRESS, out List<IpAdressXCountriesViewModel> ipadressCache))
                        {
                            var cachedIp = ipadressCache.FirstOrDefault(x => x.IP == existingRecord.IP);

                            if (cachedIp != null)
                            {
                                cachedIp.IP = updatedInfo.IP;
                                cachedIp.CountryName = updatedInfo.CountryName;
                                cachedIp.TwoLetterCode = updatedInfo.TwoLetterCode;
                                cachedIp.ThreeLetterCode = updatedInfo.ThreeLetterCode;
                                _memoryCache.Set(KEY_IPADRESS, ipadressCache);

                            }
                            else
                            {
                                ipadressCache.Add(new IpAdressXCountriesViewModel
                                {
                                    IP = updatedInfo.IP,
                                    CountryName = updatedInfo.CountryName,
                                    TwoLetterCode = updatedInfo.TwoLetterCode,
                                    ThreeLetterCode = updatedInfo.ThreeLetterCode
                                });

                                _memoryCache.Set(KEY_IPADRESS, ipadressCache);
                            }
                        }
                        else
                        {
                            ipadressCache = new List<IpAdressXCountriesViewModel>
                            {
                                 new IpAdressXCountriesViewModel
                                 {
                                        IP = updatedInfo.IP,
                                        CountryName = updatedInfo.CountryName,
                                        TwoLetterCode = updatedInfo.TwoLetterCode,
                                        ThreeLetterCode = updatedInfo.ThreeLetterCode
                                 }
                            };

                            _memoryCache.Set(KEY_IPADRESS, ipadressCache);
                        }

                        _logger.LogInformation("Cache initializes with a new register: {CacheContent}", ipadressCache);

                    }
                }

                _logger.LogInformation("Update IPs Job Finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Update IPs Job.");
            }
            finally
            {
                _isJobRunning = false;
            }


        }

        public async Task<IpAdressXCountriesViewModel> GetIpAdressReq(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("IP address is required.");
            }

            var url = $"http://ip2c.org/{ip}";

            try
            {
                var result = await _httpClient.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();

                    switch (content[0])
                    {
                        case '0':
                            throw new InvalidOperationException("Something went wrong with the external API.");

                        case '1':
                            var reply = content.Split(';');


                            var countryInfo = new IpAdressXCountriesViewModel
                            {
                                IP = ip,
                                TwoLetterCode = reply[1],
                                ThreeLetterCode = reply[2],
                                CountryName = reply[3]
                            };

                            return countryInfo;

                        default:
                            throw new InvalidOperationException("Unknown response format.");
                    }
                }
                else
                {
                    throw new HttpRequestException($"External API call failed with status code {result.StatusCode}.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }


    }


}

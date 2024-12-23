using IP_Domain.Interfaces;
using IP_Domain.Entities;
using IP_DAL;
using System.Net;
using IP_Domain.ViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Microsoft.Data.SqlClient;

namespace IP_DAL.Repository
{
    public class IpAdressRepository : IIPAdressRepository
    {
        private readonly AppDbContext _context;

        public IpAdressRepository(AppDbContext context)
        {
            _context = context;
        }

        public IpAdressXCountriesViewModel GetIPAddress(string ipAddresses)
        {
            var result = (from ipadress in _context.IPAddresses
                          join countries in _context.Countries
                          on ipadress.CountryId equals countries.Id
                          where ipadress.IP == ipAddresses
                          select new IpAdressXCountriesViewModel
                          {
                              IP = ipadress.IP,
                              CountryName = countries.Name,
                              TwoLetterCode = countries.TwoLetterCode,
                              ThreeLetterCode = countries.ThreeLetterCode
                          }).FirstOrDefault();

            return result;
        }
        public IpAdressXCountriesViewModel GetIPAddressID(int id)
        {
            var result = (from ipadress in _context.IPAddresses
                          join countries in _context.Countries
                          on ipadress.CountryId equals countries.Id
                          where ipadress.Id == id
                          select new IpAdressXCountriesViewModel
                          {
                              IP = ipadress.IP,
                              CountryName = countries.Name,
                              TwoLetterCode = countries.TwoLetterCode,
                              ThreeLetterCode = countries.ThreeLetterCode
                          }).FirstOrDefault();

            return result;
        }


        public async Task AddIPAddressAsync(IpAdressXCountriesViewModel IpAdressXCountries)
        {
            var country = await _context.Countries
            .Where(e => e.TwoLetterCode == IpAdressXCountries.TwoLetterCode)
            .FirstOrDefaultAsync();

            if (country == null)
            {
                country = new Countries
                {
                    Name = IpAdressXCountries.CountryName,
                    TwoLetterCode = IpAdressXCountries.TwoLetterCode,
                    ThreeLetterCode = IpAdressXCountries.ThreeLetterCode,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Countries.Add(country);
                await _context.SaveChangesAsync();

            }
                var ipadress = new IPAddresses
                {
                    CountryId = country.Id,
                    IP = IpAdressXCountries.IP,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.IPAddresses.Add(ipadress);
                await _context.SaveChangesAsync();

            
            return;

        }

        public async Task<List<IpAdressXCountriesIDViewModel>> GetIpAddressesUpdateAsync(int limit)
        {

            var oneHourAgo = DateTimeOffset.UtcNow
            .ToOffset(TimeSpan.FromHours(-3))
            .AddHours(-1);

            var result = await (from ipadress in _context.IPAddresses
                                join countries in _context.Countries
                                on ipadress.CountryId equals countries.Id
                                select new IpAdressXCountriesIDViewModel
                                {
                                    Id = ipadress.Id,
                                    IP = ipadress.IP,
                                    CountryName = countries.Name,
                                    TwoLetterCode = countries.TwoLetterCode,
                                    ThreeLetterCode = countries.ThreeLetterCode,
                                    UpdatedAt = ipadress.UpdatedAt
                                })
                                .Where(ip => EF.Functions.DateDiffMinute(ip.UpdatedAt, DateTimeOffset.UtcNow) > 5)
                                .OrderBy(ip => ip.UpdatedAt)
                                .ThenBy(ip => ip.Id)
                                .Take(limit) 
                                .ToListAsync();

            return result;
        }


        public async Task PutIpAddressesUpdateAsync(IpAdressXCountriesIDViewModel ipAdressXCountriesIDViewModel)
        {

            var country = await _context.Countries
            .Where(e => e.TwoLetterCode == ipAdressXCountriesIDViewModel.TwoLetterCode)
            .FirstOrDefaultAsync();

            if (country == null)
            {
                if (country == null)
                {
                    country = new Countries
                    {
                        Name = ipAdressXCountriesIDViewModel.CountryName,
                        TwoLetterCode = ipAdressXCountriesIDViewModel.TwoLetterCode,
                        ThreeLetterCode = ipAdressXCountriesIDViewModel.ThreeLetterCode,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Countries.Add(country);
                    await _context.SaveChangesAsync();

                }
            }

            var entityCountry = await _context.Countries.FindAsync(country.Id);
            if(entityCountry != null)
            {
                entityCountry.TwoLetterCode = country.TwoLetterCode;
                entityCountry.ThreeLetterCode = country.ThreeLetterCode;
                entityCountry.Name = country.Name;

                _context.Countries.Update(entityCountry);
                await _context.SaveChangesAsync();
            }
           
            var entityAdreess = await _context.IPAddresses.FindAsync(ipAdressXCountriesIDViewModel.Id);
            if (entityAdreess != null)
            {
                entityAdreess.CountryId = country.Id;
                entityAdreess.UpdatedAt = ipAdressXCountriesIDViewModel.UpdatedAt;

            }
            _context.IPAddresses.Update(entityAdreess);

        }

        public List<IpCountryReportViewModel> GetIpCountryReport(string listip)
        {
            string query = @"select
                 c.Name as CountryName 
                ,Count(i.CountryId) as AdressessCount
                ,(Select max (UpdatedAt)) as LastTimeUpdated 
                From dbo.Countries c 
                Inner Join dbo.IPAddresses i 
                On c.Id = i.CountryId 
                Where (@List IS NULL OR @List = '' OR c.TwoLetterCode IN (
                Select LTRIM(RTRIM(Value)) 
                From STRING_SPLIT(@List, ',')))
                Group by Name";
            return _context.ReportIpCountry.FromSqlRaw(query, new SqlParameter("@List", listip)).ToList();

        }

    }
}

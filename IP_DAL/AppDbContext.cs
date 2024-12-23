using Microsoft.EntityFrameworkCore;
using IP_Domain.Entities;
using System.Diagnostics.Metrics;
using IP_Domain.ViewModel;

namespace IP_DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<IPAddresses> IPAddresses { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<IpCountryReportViewModel> ReportIpCountry { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IPAddresses>()
                .HasOne(p => p.Countries)
                .WithMany(c => c.Addresses)
                .HasForeignKey(p => p.CountryId);
        }

    }
}

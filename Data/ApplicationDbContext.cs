using EbillingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace EbillingV2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your DbSets here
        // public DbSet<PatientInfo> Patients { get; set; }

        public DbSet<Patient> tblsignboards { get; set; }
        public DbSet<Patient> tblstocklocation { get; set; }
        public DbSet<Patient> tblstocksublocation { get; set; }
        public DbSet<Patient> tblhospitals { get; set; }
        public DbSet<StockPatientBillingModel> tblstockpatbilling { get; set; }

        public DbSet<StockResultViewModel> StockResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your model here
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id); // Primary key

                // Configure required fields
                entity.Property(p => p.PatRefNo)
                      .IsRequired()
                      .HasMaxLength(50); // Adjust length as needed

                // Configure indexes for better query performance
                entity.HasIndex(p => p.PatRefNo);
                entity.HasIndex(p => p.HospitalId);
                entity.HasIndex(p => p.LocationId);
                entity.HasIndex(p => p.SubLocationId);
            });

            modelBuilder.Entity<StockResultViewModel>(entity =>
            {
                // Configure StockResultViewModel as needed
                // For example, mapping it to a database table
                entity.ToTable("StockResults");  // If you have a specific table for stock results
                entity.HasKey(s => s.StockCode); // Set the primary key for StockResultViewModel
            });
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ContractorDashboard.Models;

namespace ContractorDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<ContractorSettings> ContractorSettings { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<ManusCalculation> ManusCalculations { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Contractor - ContractorSettings (One-to-One)
            modelBuilder.Entity<Contractor>()
                .HasOne(c => c.Settings)
                .WithOne(s => s.Contractor)
                .HasForeignKey<ContractorSettings>(s => s.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contractor - Bids (One-to-Many)
            modelBuilder.Entity<Contractor>()
                .HasMany(c => c.Bids)
                .WithOne(b => b.Contractor)
                .HasForeignKey(b => b.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job - Bids (One-to-Many)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Bids)
                .WithOne(b => b.Job)
                .HasForeignKey(b => b.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bid - ManusCalculation (One-to-One) - FIXED
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.ManusCalculation)
                .WithOne(m => m.Bid)
                .HasForeignKey<ManusCalculation>(m => m.BidId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
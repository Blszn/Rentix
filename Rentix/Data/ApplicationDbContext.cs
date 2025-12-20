using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rentix.Models;

namespace Rentix.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Vehicle
            builder.Entity<Vehicle>(entity =>
            {
                entity.HasIndex(v => v.PlateNumber).IsUnique();
                entity.Property(v => v.DailyPrice).HasPrecision(18, 2);
            });

            // Rental
            builder.Entity<Rental>(entity =>
            {
                entity.Property(r => r.TotalPrice).HasPrecision(18, 2);

                // Araç silinirse geçmiş kiralama kayıtları bozulmasın (Restrict)
                entity.HasOne(r => r.Vehicle)
                      .WithMany(v => v.Rentals)
                      .HasForeignKey(r => r.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
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

        // --- BU KISMI GÜNCELLE ---
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. Şemayı Tanımla (Burası ÇOK ÖNEMLİ)
            // Hostingdeki tablo adların neyse (resimde rentix53 görünüyor) onu yaz.
            builder.HasDefaultSchema("rentix53");

            base.OnModelCreating(builder);

            // Senin diğer özel ayarların varsa burada durabilir...
        }
        // -------------------------

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
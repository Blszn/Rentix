using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models;
using Rentix.Models.Enums;
using System.Security.Claims;

namespace Rentix.Controllers
{
    [Authorize] // Giriş yapmayan kimse bu sayfaların hiçbirini göremez (Login'e atar)
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. KULLANICI: Kiralamalarım Listesi
        public async Task<IActionResult> MyRentals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(rentals);
        }

        // 2. KULLANICI: Kiralama Ekranını Aç (GET)
        // Search formundan gelen startDate ve endDate verilerini de kabul eder
        [HttpGet]
        public async Task<IActionResult> Rent(int? id, DateTime? startDate, DateTime? endDate)
        {
            if (id == null) return NotFound();

            // Aracı resimleriyle birlikte getir
            var vehicle = await _context.Vehicles
                .Include(v => v.Images)
                .FirstOrDefaultAsync(v => v.Id == id);

            // Araç yoksa veya müsait değilse Galeriye at
            if (vehicle == null || (vehicle.Status != VehicleStatus.Available && vehicle.Status != VehicleStatus.Rented))
            {
                return RedirectToAction("Gallery", "Home");
            }

            // Arama formundan tarih geldiyse onu kullan, yoksa bugünü seç
            var sDate = startDate ?? DateTime.Today;
            var eDate = endDate ?? DateTime.Today.AddDays(1);

            // Tarih kontrolü (Eski tarih gelirse düzelt)
            if (sDate < DateTime.Today) sDate = DateTime.Today;
            if (eDate <= sDate) eDate = sDate.AddDays(1);

            // Modeli doldurup View'a gönder
            var model = new Rental
            {
                VehicleId = vehicle.Id,
                Vehicle = vehicle,
                StartDate = sDate,
                EndDate = eDate
            };
            return View(model);
        }

        // 3. KULLANICI: Kiralama İşlemini Onayla (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(Rental rental)
        {
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle == null) return NotFound();

            // Validasyon temizliği
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Vehicle");
            ModelState.Remove("RentalStatus");

            // Tarih ve Çakışma Kontrolleri
            if (rental.StartDate < DateTime.Today)
                ModelState.AddModelError("StartDate", "Geçmişe tarihli kiralama yapılamaz.");

            if (rental.EndDate <= rental.StartDate)
                ModelState.AddModelError("EndDate", "Bitiş tarihi başlangıçtan ileri olmalıdır.");

            // Veritabanında tarih çakışması var mı?
            bool isConflict = await _context.Rentals.AnyAsync(r =>
                r.VehicleId == rental.VehicleId &&
                r.RentalStatus == RentalStatus.Active &&
                (rental.StartDate < r.EndDate && r.StartDate < rental.EndDate));

            if (isConflict)
                ModelState.AddModelError("", "Seçilen tarih aralığında bu araç maalesef dolu.");

            if (ModelState.IsValid)
            {
                // Fiyat Hesapla
                var days = (rental.EndDate - rental.StartDate).Days;
                if (days == 0) days = 1;
                rental.TotalPrice = days * vehicle.DailyPrice;

                // Kullanıcıyı ata
                rental.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                rental.RentalStatus = RentalStatus.Active;

                // ARAÇ DURUMUNU GÜNCELLE
                vehicle.Status = VehicleStatus.Rented;
                _context.Update(vehicle);

                // SQL Hatası önlemek için ID sıfırla
                rental.Id = 0;
                _context.Add(rental);

                // Log Tut
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = rental.UserId,
                    Action = "Rent",
                    EntityName = $"Vehicle-{vehicle.PlateNumber}",
                    Date = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyRentals));
            }

            // Hata varsa aracı tekrar modele yükle ve sayfayı göster
            rental.Vehicle = vehicle;
            return View(rental);
        }

        // 4. KULLANICI: Kiralamayı İptal Et (Cancel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRental(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            // Başkasının malını iptal edemesin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (rental.UserId != userId) return Unauthorized();

            if (rental.RentalStatus == RentalStatus.Active)
            {
                rental.RentalStatus = RentalStatus.Cancelled;

                // Aracı tekrar müsait yap
                if (rental.Vehicle != null)
                {
                    rental.Vehicle.Status = VehicleStatus.Available;
                    _context.Update(rental.Vehicle);
                }
                _context.Update(rental);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyRentals));
        }

        // 5. ADMIN: Aracı Teslim Al (Return)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null) return NotFound();

            if (rental.RentalStatus == RentalStatus.Active)
            {
                rental.RentalStatus = RentalStatus.Completed;

                // Aracı tekrar müsait yap
                if (rental.Vehicle != null)
                {
                    rental.Vehicle.Status = VehicleStatus.Available;
                    _context.Update(rental.Vehicle);
                }

                _context.Update(rental);
                await _context.SaveChangesAsync();
            }

            // Admin paneline geri dön
            return RedirectToAction("Index", "Admin");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models.Enums;

namespace Rentix.Controllers
{
    // Not: Class seviyesinde [Authorize] YOK. Herkes girebilir.
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Admin ise panele, deðilse vitrine
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        // GALERÝ: Tüm araçlarý veya arama sonuçlarýný listeler
        [HttpGet]
        public async Task<IActionResult> Gallery()
        {
            // Sadece statüsü 'Available' (Müsait) olanlarý getir
            var vehicles = await _context.Vehicles
                .Include(v => v.Images)
                .Where(v => v.Status == VehicleStatus.Available)
                .ToListAsync();

            return View(vehicles);
        }

        // ARAMA ÝÞLEMÝ (YENÝ EKLENEN KISIM)
        // Ana sayfadaki form buraya tarihleri gönderir
        [HttpGet]
        public async Task<IActionResult> Search(DateTime startDate, DateTime endDate, string location)
        {
            // Basit tarih kontrolü
            if (startDate < DateTime.Today) startDate = DateTime.Today;
            if (endDate <= startDate) endDate = startDate.AddDays(1);

            // Veritabanýndaki TÜM araçlarý çek (önce filtrelemeden)
            var allVehicles = await _context.Vehicles
                .Include(v => v.Images)
                .Include(v => v.Rentals) // Kiralama geçmiþini kontrol edeceðiz
                .ToListAsync();

            // LÝSTE FÝLTRELEME:
            // "Seçilen tarihlerde, aktif bir kiralamasýyla çakýþan araçlarý ELÝYORUZ"
            var availableVehicles = allVehicles.Where(vehicle =>
            {
                // 1. Araç genel olarak serviste veya pasifse gösterme
                if (vehicle.Status != VehicleStatus.Available && vehicle.Status != VehicleStatus.Rented)
                    return false;

                // 2. Çakýþma Kontrolü:
                // Bu aracýn aktif kiralamalarýndan herhangi biri, seçilen tarih aralýðýyla çakýþýyor mu?
                bool isBooked = vehicle.Rentals.Any(r =>
                    r.RentalStatus == RentalStatus.Active && // Sadece aktif kiralamalar
                    (startDate < r.EndDate && r.StartDate < endDate) // Tarih çakýþma formülü
                );

                // Eðer çakýþma YOKSA (isBooked == false) bu aracý listeye ekle
                return !isBooked;
            }).ToList();

            // Kullanýcýya bilgi vermek için tarihleri View'a taþýyalým
            ViewBag.SearchStart = startDate;
            ViewBag.SearchEnd = endDate;

            // Filtrelenmiþ listeyi Gallery sayfasýna gönder
            return View("Gallery", availableVehicles);
        }
    }
}
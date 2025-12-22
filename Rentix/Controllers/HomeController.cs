using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models; // Rental sınıfı için
using Rentix.Models.Enums;

namespace Rentix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Gallery()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Images)
                .Where(v => v.Status == VehicleStatus.Available)
                .ToListAsync();

            return View(vehicles);
        }

        // --- GÜNCELLENMİŞ HATA YAKALAYICILI ARAMA METODU ---
        [HttpGet]
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string location)
        {
            try
            {
                // 1. TARİH KONTROLÜ (Null gelirse varsayılan ata)
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? start.AddDays(1);

                if (start < DateTime.Today) start = DateTime.Today;
                if (end <= start) end = start.AddDays(1);

                // 2. VERİTABANI KONTROLÜ
                if (_context.Vehicles == null)
                {
                    throw new Exception("Veritabanı bağlantısı başarılı ancak 'Vehicles' tablosu NULL dönüyor.");
                }

                // 3. VERİLERİ ÇEKME (Include ile ilişkileri doldur)
                var allVehicles = await _context.Vehicles
                    .Include(v => v.Images)
                    .Include(v => v.Rentals)
                    .ToListAsync();

                // 4. FİLTRELEME MANTIĞI (Defensive Coding - Hata Önleyici)
                var availableVehicles = allVehicles.Where(vehicle =>
                {
                    // Araç bozuksa veya bakımda ise gösterme
                    if (vehicle.Status != VehicleStatus.Available && vehicle.Status != VehicleStatus.Rented)
                        return false;

                    // Kiralama listesi NULL gelirse boş liste kabul et (Patlamayı önler)
                    var rentals = vehicle.Rentals ?? new List<Rental>();

                    // Çakışma Kontrolü
                    bool isBooked = rentals.Any(r =>
                        r.RentalStatus == RentalStatus.Active && // Sadece aktif kiralamalar
                        (start < r.EndDate && r.StartDate < end) // Tarih Çakışma Formülü
                    );

                    // Eğer rezerve değilse (!isBooked) listeye ekle
                    return !isBooked;
                }).ToList();

                // View'a tarihleri gönder
                ViewBag.SearchStart = start;
                ViewBag.SearchEnd = end;

                // Sonuçları Gallery sayfasına bas
                return View("Gallery", availableVehicles);
            }
            catch (Exception ex)
            {
                // --- HATA EKRANI OLUŞTURMA (HTML) ---
                // Mobilde dosya inmemesi için ContentType 'text/html' yapıldı.

                string errorHtml = $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Hata Raporu</title>
                    <style>
                        body {{ background-color: #121212; color: #e0e0e0; font-family: 'Consolas', 'Monaco', monospace; padding: 20px; }}
                        h1 {{ color: #ff5252; border-bottom: 2px solid #ff5252; padding-bottom: 10px; }}
                        .box {{ background: #1e1e1e; padding: 15px; margin-bottom: 20px; border-radius: 8px; border: 1px solid #333; overflow-x: auto; }}
                        .label {{ color: #82b1ff; font-weight: bold; margin-bottom: 5px; display: block; }}
                        .msg {{ color: #fff; }}
                        .stack {{ color: #69f0ae; font-size: 13px; }}
                    </style>
                </head>
                <body>
                    <h1>⚠️ SİSTEM HATASI (DEBUG)</h1>
                    
                    <span class='label'>HATA MESAJI:</span>
                    <div class='box msg'>{ex.Message}</div>

                    <span class='label'>NEREDE OLDU (StackTrace):</span>
                    <div class='box stack'><pre>{ex.StackTrace}</pre></div>

                    <span class='label'>DETAY (Inner Exception):</span>
                    <div class='box msg'>{(ex.InnerException != null ? ex.InnerException.Message : "Yok")}</div>
                    
                    <p style='color: #888;'>* Bu ekran sadece hatayı bulmak içindir. Hata çözüldüğünde bu kod kaldırılmalıdır.</p>
                </body>
                </html>";

                return Content(errorHtml, "text/html");
            }
        }
    }
}
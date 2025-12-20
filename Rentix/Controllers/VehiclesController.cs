using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models;
using Rentix.Models.Enums;
using System.Security.Claims;

namespace Rentix.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VehiclesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vehicles.ToListAsync());
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle, List<IFormFile> imgFiles)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Vehicles.AnyAsync(v => v.PlateNumber == vehicle.PlateNumber))
                {
                    ModelState.AddModelError("PlateNumber", "Bu plaka zaten kayıtlı.");
                    return View(vehicle);
                }

                // Resim Yükleme
                if (imgFiles != null && imgFiles.Count > 0)
                {
                    foreach (var file in imgFiles)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "img", "vehicles");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                        string filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        vehicle.Images.Add(new VehicleImage { ImageUrl = "/img/vehicles/" + fileName });
                    }
                }

                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                await LogActionAsync("Create", "Vehicle", $"Araç eklendi: {vehicle.PlateNumber}");
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Düzenleme sayfasına girerken resimleri de getiriyoruz (Include)
            var vehicle = await _context.Vehicles
                .Include(v => v.Images)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5 (GÜNCELLENMİŞ TAM HALİ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle, List<IFormFile> newImgFiles, List<int> deleteImageIds)
        {
            if (id != vehicle.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Mevcut aracı veritabanından resimleriyle birlikte çekiyoruz
                    // (Formdan gelen 'vehicle' nesnesi eksik olabilir, veritabanı en doğrusudur)
                    var existingVehicle = await _context.Vehicles
                        .Include(v => v.Images)
                        .FirstOrDefaultAsync(v => v.Id == id);

                    if (existingVehicle == null) return NotFound();

                    // 2. Temel bilgileri güncelle
                    existingVehicle.Brand = vehicle.Brand;
                    existingVehicle.Model = vehicle.Model;
                    existingVehicle.PlateNumber = vehicle.PlateNumber;
                    existingVehicle.Year = vehicle.Year;
                    existingVehicle.DailyPrice = vehicle.DailyPrice;
                    existingVehicle.Status = vehicle.Status;

                    // 3. RESİM SİLME İŞLEMİ
                    if (deleteImageIds != null && deleteImageIds.Count > 0)
                    {
                        // Silinecek resimleri seç
                        var imagesToDelete = existingVehicle.Images
                            .Where(img => deleteImageIds.Contains(img.Id))
                            .ToList();

                        foreach (var img in imagesToDelete)
                        {
                            // A) Sunucudan dosyayı sil (wwwroot içinden)
                            // ImageUrl: "/img/vehicles/resim.jpg" -> Başındaki "/" işaretini kaldırıp path oluştur
                            string filePath = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            // B) Veritabanından sil
                            _context.VehicleImages.Remove(img);
                        }
                    }

                    // 4. YENİ RESİM EKLEME İŞLEMİ
                    if (newImgFiles != null && newImgFiles.Count > 0)
                    {
                        foreach (var file in newImgFiles)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "img", "vehicles");
                            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                            string filePath = Path.Combine(uploadPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            existingVehicle.Images.Add(new VehicleImage { ImageUrl = "/img/vehicles/" + fileName });
                        }
                    }

                    // Değişiklikleri kaydet
                    _context.Update(existingVehicle);
                    await _context.SaveChangesAsync();

                    await LogActionAsync("Update", "Vehicle", $"Araç güncellendi: {vehicle.PlateNumber}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. ADIM: Araç ŞU ANDA kirada mı kontrol et?
            // Sadece 'Active' (Kirada) durumunda olan kayıt var mı diye bakıyoruz.
            bool isCurrentlyRented = await _context.Rentals.AnyAsync(r =>
                r.VehicleId == id &&
                r.RentalStatus == RentalStatus.Active);

            if (isCurrentlyRented)
            {
                // Eğer araç şu an müşterideyse, silme işlemini durdur ve uyarı ver.
                var v = await _context.Vehicles.FindAsync(id);
                ViewBag.ErrorMessage = "DİKKAT: Bu araç şu anda bir müşteride (KİRADA) görünüyor. Silmek için önce aracı teslim almalısınız.";
                return View(v);
            }

            // 2. ADIM: Aracı, Resimlerini ve GEÇMİŞ Kiralamalarını getir
            var vehicle = await _context.Vehicles
                .Include(v => v.Images)   // Resimleri getir
                .Include(v => v.Rentals)  // Geçmiş kiralama kayıtlarını getir (Foreign Key hatasını önlemek için)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // A) Resim Dosyalarını Sunucudan Temizle
                foreach (var img in vehicle.Images)
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // B) Geçmiş Kiralama Kayıtlarını Veritabanından Sil
                // (Araç şu an kirada olmadığına göre, buradaki kayıtlar eski/tamamlanmış kayıtlardır)
                if (vehicle.Rentals != null && vehicle.Rentals.Any())
                {
                    _context.Rentals.RemoveRange(vehicle.Rentals);
                }

                // C) Aracı Sil
                _context.Vehicles.Remove(vehicle);

                await _context.SaveChangesAsync();

                // İstersen "Araç ve geçmişi silindi" diye log atabilirsin
                // await LogActionAsync("Delete", "Vehicle", $"Araç ve geçmişi silindi: {vehicle.PlateNumber}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Beklenmedik bir hata olursa
                ViewBag.ErrorMessage = "Silme işlemi sırasında beklenmedik bir hata oluştu: " + ex.Message;
                return View(vehicle);
            }
        }
        private bool VehicleExists(int id) => _context.Vehicles.Any(e => e.Id == id);

        private async Task LogActionAsync(string action, string entity, string desc)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.AuditLogs.Add(new AuditLog { UserId = userId, Action = action, EntityName = entity, Date = DateTime.Now });
            await _context.SaveChangesAsync();
        }
    }
}
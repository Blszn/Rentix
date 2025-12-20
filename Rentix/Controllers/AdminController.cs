using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models.Enums;

namespace Rentix.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin girebilir
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. İstatistikler (Dashboard üstü için)
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.ActiveRentals = await _context.Rentals.CountAsync(r => r.RentalStatus == RentalStatus.Active);
            ViewBag.TotalUsers = await _context.Users.CountAsync(); // IdentityUser tablosu

            // 2. KİRADA OLAN ARAÇLAR LİSTESİ
            // Hem Aracı (Vehicle) hem de Kiralayanı (User) getiriyoruz.
            var activeRentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User) // Kullanıcı bilgilerini çekmek için ŞART
                .Where(r => r.RentalStatus == RentalStatus.Active)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(activeRentals);
        }
    }
}
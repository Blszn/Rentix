using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;

namespace Rentix.Controllers
{
    [Authorize(Roles = "Admin")] // Kesinlikle sadece Admin
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Logları tarihe göre tersten sırala (En yeni en üstte)
            // User tablosunu da Include et ki kimin yaptığını görelim (Email vs.)
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(logs);
        }
    }
}
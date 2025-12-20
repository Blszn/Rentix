using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;
using Rentix.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// PostgreSQL Kullanýmý (UseNpgsql olmalý)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Identity Ayarlarý (Rol Desteði Aktif)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Geliþtirme kolaylýðý için þifre kurallarýný gevþettik
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 3. Admin Seed Ýþlemi (Otomatik Admin Oluþturma)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // DbSeeder sýnýfýnýn projenizde olduðundan emin olun
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed iþlemi sýrasýnda hata oluþtu.");
    }
}

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Veritabanýný oluþtur ve bekleyen göçleri (migration) uygula
    dataContext.Database.Migrate();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
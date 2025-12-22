using Rentix.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Rentix.Data;       // DÜZELTÝLDÝ: AKTES -> Rentix oldu
using Rentix.Models;     // DÜZELTÝLDÝ: AKTES -> Rentix oldu
using Rentix.Services;   // DÜZELTÝLDÝ: EmailSender'ý bulmasý için

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Veritabaný Servisi
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// 2. Identity (Üyelik) Ayarlarý
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // E-posta onayý AÇIK (True)
    options.SignIn.RequireConfirmedAccount = true;

    // Þifre Ayarlarý
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// 3. Email Servisi
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 4. MVC ve Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 5. Pipeline Ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Rentix.Models; // ApplicationUser burada
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Rentix.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        // Formdan gelecek veriler burada tanımlanır
        public class InputModel
        {
            [Required(ErrorMessage = "Ad alanı zorunludur.")]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyad alanı zorunludur.")]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Şifre Tekrar")]
            [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // YENİ KULLANICI OLUŞTURMA ANI
                var user = new ApplicationUser
                {
                    UserName = Input.Email, // Giriş yaparken Email kullanılsın diye UserName'i Email yapıyoruz
                    Email = Input.Email,
                    FirstName = Input.FirstName, // Yeni alan
                    LastName = Input.LastName    // Yeni alan
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Kayıt başarılı, otomatik giriş yap
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                // Hata varsa (örn: şifre basitse) ekrana bas
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Hata varsa sayfayı tekrar göster
            return Page();
        }
    }
}
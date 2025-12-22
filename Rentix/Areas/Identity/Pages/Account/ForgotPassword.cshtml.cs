using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Rentix.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Rentix.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                // Güvenlik: Kullanıcı yoksa veya e-postası onaylı değilse bile "Gönderdik" deriz.
                // Böylece kötü niyetli kişiler hangi mailin kayıtlı olduğunu anlayamaz.
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Kullanıcıyı direkt onay sayfasına gönder
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Token (Anahtar) Oluşturma
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Link Oluşturma
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Mail Gönderme
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Şifre Sıfırlama Talebi",
                    $"Şifrenizi sıfırlamak için lütfen <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya tıklayınız</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
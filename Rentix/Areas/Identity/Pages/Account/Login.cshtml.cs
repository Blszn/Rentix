// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Rentix.Models;

namespace Rentix.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager; // EKLENDİ: Rol kontrolü için gerekli
        private readonly ILogger<LoginModel> _logger;

        // Constructor güncellendi: UserManager eklendi
        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Giriş yapmayı dene
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // --- ÖZELLEŞTİRİLMİŞ YÖNLENDİRME BAŞLANGIÇ ---

                    // Giriş yapan kullanıcıyı bul
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (user != null)
                    {
                        // Admin ise Yönetim Paneline (Vehicles/Index)
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Vehicles", new { area = "" });
                        }
                        // Normal kullanıcı ise Galeriye (Home/Gallery)
                        else
                        {
                            return RedirectToAction("Gallery", "Home", new { area = "" });
                        }
                    }

                    // Herhangi bir sorun olursa varsayılan yola git
                    return LocalRedirect(returnUrl);

                    // --- ÖZELLEŞTİRİLMİŞ YÖNLENDİRME BİTİŞ ---
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
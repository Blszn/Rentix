using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Rentix.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // appsettings.json dosyasından ayarları oku
            var smtpServer = _configuration["EmailSettings:Server"];
            var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var smtpUser = _configuration["EmailSettings:User"];
            var smtpPass = _configuration["EmailSettings:Password"];

            // Eğer ayarlar boşsa (Henüz hostinge geçilmediyse) sahte işlem yap
            // Bu sayede proje patlamaz, sadece konsola yazar.
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
            {
                System.Diagnostics.Debug.WriteLine($"[SAHTE EMAIL GÖNDERİMİ] Kime: {email}, Konu: {subject}");
                System.Diagnostics.Debug.WriteLine($"İçerik: {htmlMessage}");
                return; // Gerçek mail göndermeden çık
            }

            try
            {
                var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true // Genelde true olur
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, "Rentix Destek"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Hata olursa loglayalım ama kullanıcıya çaktırmayalım (veya hata sayfasına yönlendirebilirsin)
                System.Diagnostics.Debug.WriteLine($"Email Gönderme Hatası: {ex.Message}");
                throw; // İstersen bu throw'u kaldırabilirsin, uygulama durmasın diye.
            }
        }
    }
}
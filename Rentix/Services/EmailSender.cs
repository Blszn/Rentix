using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
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

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailHost = _configuration["EmailSettings:Host"];

            // Eğer Port okunamazsa varsayılan 587 olsun
            var portAyari = _configuration["EmailSettings:Port"];
            var mailPort = string.IsNullOrEmpty(portAyari) ? 587 : int.Parse(portAyari);

            var mailAdres = _configuration["EmailSettings:Mail"];
            var mailSifre = _configuration["EmailSettings:Password"];
            var gorunenAd = _configuration["EmailSettings:DisplayName"];

            // Şifre boşsa hata fırlat (Debug için)
            if (string.IsNullOrEmpty(mailSifre))
            {
                throw new Exception("Mail şifresi appsettings.json dosyasından okunamadı! Lütfen kontrol edin.");
            }

            try
            {
                var client = new SmtpClient(mailHost, mailPort)
                {
                    // Bazı hostinglerde burası false, bazılarında true olmalı. 
                    // Önce true dene, gitmezse false yapıp dene.
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mailAdres, mailSifre),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mailAdres, gorunenAd),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                return client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Hatanın detayını görmek için
                throw new InvalidOperationException($"E-posta gönderilemedi. Hata: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }
    }
}
using System.Net.Mail;
using System.Net;
using TicketBookingService.Abstract;

namespace TicketBookingService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {

                var smtpHost = _config["Smtp:Host"]; // e.g., smtp.gmail.com
                var smtpPort = int.Parse(_config["Smtp:Port"]); // e.g., 587
                var fromEmail = _config["Smtp:From"];
                var username = _config["Smtp:Username"];
                var password = _config["Smtp:Password"];

                var mail = new MailMessage(fromEmail, to, subject, body)
                {
                    IsBodyHtml = true // or false for plain text
                };

                using var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

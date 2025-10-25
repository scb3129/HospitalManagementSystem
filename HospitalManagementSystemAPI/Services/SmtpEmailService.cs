using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace HospitalManagementSystemAPI.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly bool _useSsl;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;

            _smtpServer = _config["Smtp:Host"];
            _smtpPort = int.Parse(_config["Smtp:Port"]);
            _useSsl = bool.Parse(_config["Smtp:UseSsl"]);
            _smtpUser = _config["Smtp:Username"];
            _smtpPass = _config["Smtp:Password"];
            _fromEmail = _config["Smtp:FromEmail"];
            _fromName = _config["Smtp:FromName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_fromName, _fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTlsWhenAvailable);
                await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ SMTP Email sending failed: " + ex.Message);
            }
        }
    }
}

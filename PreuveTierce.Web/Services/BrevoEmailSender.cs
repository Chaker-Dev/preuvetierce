using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Services
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BrevoEmailSender> _logger;
        public BrevoEmailSender(IConfiguration config,ILogger<BrevoEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Envoi email via SMTP Brevo vers {Email}", email);

            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    _config["EmailSettings:SenderName"],
                    _config["EmailSettings:SenderEmail"]
                ));

                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    string smtpServer = _config["EmailSettings:Server"];
                    int smtpPort = int.Parse(_config["EmailSettings:Port"]);

                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    string smtpUser = _config["EmailSettings:Username"];
                    string smtpPass = _config["EmailSettings:Password"];

                    await client.AuthenticateAsync(smtpUser, smtpPass);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation("Email envoyé avec succès.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur critique lors de l'envoi SMTP vers {Email}", email);
                throw; 
            }
        }

    }
}

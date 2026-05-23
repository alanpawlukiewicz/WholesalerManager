using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Configuration;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string? email, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Tried to send information to an empty e-mail.");
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                var socketOption = _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, socketOption);

                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                await client.SendAsync(message);

                _logger.LogInformation("E-mail named '{Subject}' has been sent to: {Email}", subject, email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There has been a critical error when trying to send an e-mail: {Email}", email);
                return false;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}

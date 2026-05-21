using Microsoft.Extensions.Logging;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string? email, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            _logger.LogInformation("Attempting to send email to {Email} with body '{body}'", email, body);
            return true;
        }
    }
}

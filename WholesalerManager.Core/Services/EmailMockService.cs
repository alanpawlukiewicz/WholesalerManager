using Microsoft.Extensions.Logging;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class EmailMockService : IEmailService
    {
        private readonly ILogger<EmailMockService> _logger;

        public EmailMockService(ILogger<EmailMockService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string? email, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Tried to send information to an empty e-mail.");
                return false;
            }

            _logger.LogInformation("Attempting to send email to {Email} with body '{body}'", email, body);
            return true;
        }
    }
}

namespace WholesalerManager.Core.ServiceContracts
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string? email, string subject, string body);
    }
}

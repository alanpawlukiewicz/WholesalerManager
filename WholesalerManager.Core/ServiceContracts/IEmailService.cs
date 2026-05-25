namespace WholesalerManager.Core.ServiceContracts
{
    /// <summary>
    /// Represents a contract for an email service that provides functionality to send emails asynchronously. This interface defines a method for sending emails, which can be implemented by various email service providers or custom implementations to facilitate email communication within the application.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Asynchronously sends an email to the specified recipient with the given subject and body. This method is designed to handle email sending operations, allowing for integration with various email providers or services. The implementation of this method should ensure that emails are sent reliably and efficiently, handling any potential exceptions or errors that may arise during the sending process.
        /// </summary>
        /// <param name="email">Email address of the recipient</param>
        /// <param name="subject">Subject of the email</param>
        /// <param name="body">Body content of the email</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the email was sent successfully.</returns>
        Task<bool> SendEmailAsync(string? email, string subject, string body);
    }
}

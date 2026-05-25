using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents a repository contract for managing audit logs in the application. This interface defines methods for adding and retrieving audit logs, which are essential for tracking user actions and system events for security and compliance purposes.
    /// </summary>
    public interface IAuditLogsRepository
    {
        /// <summary>
        /// Asynchronously adds a new audit log entry to the repository. This method is used to record user actions, login attempts, and other significant events that need to be tracked for security and auditing purposes. The method returns a boolean value indicating whether the operation was successful.
        /// </summary>
        /// <param name="auditLog">The audit log entry to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a boolean value indicating whether the operation was successful.</returns>
        Task<bool> AddAuditLogAsync(AuditLog auditLog);

        /// <summary>
        /// Asynchronously retrieves all audit logs from the repository. This method is used to fetch a list of all recorded audit logs for review and analysis.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of audit logs.</returns>
        Task<List<AuditLog>> GetAllAuditLogsAsync();
    }
}

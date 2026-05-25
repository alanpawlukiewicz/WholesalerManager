using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts
{
    /// <summary>
    /// Represents a contract for an audit logger service that provides functionality to log login attempts and retrieve audit logs. This interface defines methods for logging login attempts, retrieving all audit logs, and filtering or sorting audit logs based on specified criteria.
    /// </summary>
    public interface IAuditLoggerService
    {
        /// <summary>
        /// Asynchronously logs a login attempt by recording the user ID (if available), the attempted username, and whether the login attempt was successful. This method is designed to capture important information about login activities for auditing purposes, allowing for monitoring and analysis of login patterns and potential security issues. The implementation of this method should ensure that the logged information is stored securely and can be retrieved for future reference or analysis.
        /// </summary>
        /// <param name="userID">The ID of the user attempting to log in</param>
        /// <param name="attemptedUsername">The username that was attempted to be used for login</param>
        /// <param name="success">Indicates whether the login attempt was successful</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the login attempt was logged successfully.</returns>
        Task<bool> LogLoginAttempt(Guid? userID, string attemptedUsername, bool success);

        /// <summary>
        /// Asynchronously retrieves a list of all audit logs. This method is designed to provide access to the recorded login attempts and other relevant audit information for analysis and monitoring purposes. The implementation should ensure that the retrieved audit logs are accurate and can be used for security audits, compliance checks, or any other necessary evaluations related to user activities and system access.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of audit logs.</returns>
        Task<List<AuditLog>> GetAuditLogsAsync();

        /// <summary>
        /// Retrieves a list of audit logs filtered by a specified property and filter value.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter by</param>
        /// <param name="filter">The filter value</param>
        /// <param name="ignoreCase">Indicates whether to perform case-insensitive filtering</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of filtered audit logs.</returns>
        Task<List<AuditLog>> GetFilteredAuditLogs(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Retrieves a list of audit logs sorted by a specified property and sort order.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by</param>
        /// <param name="sortOrder">The sort order</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of sorted audit logs.</returns>
        Task<List<AuditLog>> GetSortedAuditLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

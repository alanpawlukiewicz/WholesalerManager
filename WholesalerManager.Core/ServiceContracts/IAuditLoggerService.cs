using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts
{
    public interface IAuditLoggerService
    {
        Task<bool> LogLoginAttempt(Guid? userID, string attemptedUsername, bool success);

        Task<List<AuditLog>> GetAuditLogsAsync();

        Task<List<AuditLog>> GetFilteredAuditLogs(string? propertyName, string? filter, bool ignoreCase = true);

        Task<List<AuditLog>> GetSortedAuditLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

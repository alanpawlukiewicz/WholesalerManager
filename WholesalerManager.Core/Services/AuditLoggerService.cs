using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class AuditLoggerService : IAuditLoggerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogsRepository _logsRepository;

        private readonly ILogger<AuditLoggerService> _logger;

        public AuditLoggerService(IHttpContextAccessor httpContextAccessor, IAuditLogsRepository auditLogsRepository, ILogger<AuditLoggerService> logger)
        {
            _logsRepository = auditLogsRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAuditLogsAsync), nameof(AuditLoggerService));

            return await _logsRepository.GetAllAuditLogsAsync();
        }

        public async Task<List<AuditLog>> GetFilteredAuditLogs(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredAuditLogs), nameof(AuditLoggerService));

            var allLogs = await _logsRepository.GetAllAuditLogsAsync();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                _logger.LogInformation("{methodName} from {serviceName} returning all products.", nameof(GetFilteredAuditLogs), nameof(AuditLoggerService));
                return allLogs;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(AuditLog.Timestamp):
                    if (DateTime.TryParse(filter, out var filterDate))
                    {
                        var startDate = filterDate.Date;
                        var endDate = startDate.AddDays(1);

                        return allLogs.Where(l => l.Timestamp >= startDate && l.Timestamp < endDate).ToList();
                    }
                    return new List<AuditLog>();
                case nameof(AuditLog.User.Email):
                    return allLogs.Where(l => l.User != null
                    && l.User.Email != null
                    && l.User.Email.Contains(filter, stringComparisonType)).ToList();
                case nameof(AuditLog.AttemptedUsername):
                    return allLogs.Where(l => l.AttemptedUsername != null
                    && l.AttemptedUsername.Contains(filter, stringComparisonType)).ToList();
                case nameof(AuditLog.IpAddress):
                    return allLogs.Where(l => l.IpAddress != null
                    && l.IpAddress.Contains(filter, stringComparisonType)).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<List<AuditLog>> GetSortedAuditLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedAuditLogs), nameof(AuditLoggerService));

            var allLogs = await _logsRepository.GetAllAuditLogsAsync();

            if (string.IsNullOrEmpty(propertyName))
            {
                return allLogs;
            }

            switch (propertyName)
            {
                case nameof(AuditLog.Timestamp):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.Timestamp).ToList()
                        : allLogs.OrderByDescending(d => d.Timestamp).ToList();
                case nameof(AuditLog.User.Email):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.User?.Email).ToList()
                        : allLogs.OrderByDescending(d => d.User?.Email).ToList();
                case nameof(AuditLog.AttemptedUsername):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.AttemptedUsername).ToList()
                        : allLogs.OrderByDescending(d => d.AttemptedUsername).ToList();
                case nameof(AuditLog.IpAddress):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.IpAddress).ToList()
                        : allLogs.OrderByDescending(d => d.IpAddress).ToList();
                case nameof(AuditLog.Success):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.Success).ToList()
                        : allLogs.OrderByDescending(d => d.Success).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<bool> LogLoginAttempt(Guid? userID, string attemptedUsername, bool success)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(LogLoginAttempt), nameof(AuditLoggerService));

            AuditLog newLog = new AuditLog()
            {
                AuditLogID = Guid.NewGuid(),
                Success = success,
                Timestamp = DateTime.Now,
                UserID = userID,
                AttemptedUsername = attemptedUsername,
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };

            return await _logsRepository.AddAuditLogAsync(newLog);
        }
    }
}

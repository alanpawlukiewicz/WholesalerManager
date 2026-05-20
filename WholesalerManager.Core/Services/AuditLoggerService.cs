using Microsoft.AspNetCore.Http;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class AuditLoggerService : IAuditLoggerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogsRepository _logsRepository;

        public AuditLoggerService(IHttpContextAccessor httpContextAccessor, IAuditLogsRepository auditLogsRepository)
        {
            _logsRepository = auditLogsRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync()
        {
            return await _logsRepository.GetAllAuditLogsAsync();
        }

        public async Task<bool> LogLoginAttempt(Guid? userID, string attemptedUsername, bool success)
        {
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.Services.DeliveryServices;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class AuditLogsRepository : IAuditLogsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuditLogsRepository> _logger;

        public AuditLogsRepository(ApplicationDbContext db, ILogger<AuditLogsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> AddAuditLogAsync(AuditLog auditLog)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddAuditLogAsync), nameof(AuditLogsRepository));
            try
            {
                var result = await _db.AuditLog.AddAsync(auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to log login attemp given exception: {ex}", ex);
                return false;
            }

            var rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<AuditLog>> GetAllAuditLogsAsync()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllAuditLogsAsync), nameof(AuditLogsRepository));
            return await _db.AuditLog.Include("User").ToListAsync();
        }
    }
}

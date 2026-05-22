using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class InventoryLogsRepository : IInventoryLogsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<InventoryLogsRepository> _logger;

        public InventoryLogsRepository(ApplicationDbContext db, ILogger<InventoryLogsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<InventoryLog>> GetAllInventoryLogsAsync()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllInventoryLogsAsync), nameof(InventoryLogsRepository));
            return await _db.InventoryLog
                .Include("Product")
                .Include("Delivery")
                .Include("Order")
                .ToListAsync();
        }
    }
}

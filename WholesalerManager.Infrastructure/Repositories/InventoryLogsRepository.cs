using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class InventoryLogsRepository : IInventoryLogsRepository
    {
        private readonly ApplicationDbContext _db;

        public InventoryLogsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<InventoryLog>> GetAllInventoryLogsAsync()
        {
            return await _db.InventoryLog
                .Include("Product")
                .Include("Delivery")
                .Include("Order")
                .ToListAsync();
        }
    }
}

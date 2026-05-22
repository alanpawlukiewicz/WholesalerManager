using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class SuppliersRepository : ISuppliersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SuppliersRepository> _logger;

        public SuppliersRepository(ApplicationDbContext db, ILogger<SuppliersRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Supplier>> GetAllSuppliers()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllSuppliers), nameof(SuppliersRepository));

            return await _db.Supplier.ToListAsync();
        }

        public async Task<Supplier?> GetSupplierByID(Guid supplierID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSupplierByID), nameof(SuppliersRepository));

            return await _db.Supplier.FirstOrDefaultAsync(s => s.SupplierID == supplierID);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class SuppliersRepository : ISuppliersRepository
    {
        private readonly ApplicationDbContext _db;

        public SuppliersRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Supplier>> GetAllSuppliers()
        {
            return await _db.Supplier.ToListAsync();
        }

        public async Task<Supplier?> GetSupplierByID(Guid supplierID)
        {
            return await _db.Supplier.FirstOrDefaultAsync(s => s.SupplierID == supplierID);
        }
    }
}

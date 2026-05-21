using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesaleManager.Infrastructure.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoriesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _db.Category.ToListAsync();
        }
    }
}

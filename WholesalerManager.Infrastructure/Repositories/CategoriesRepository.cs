using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;
using WholesalerManager.Infrastructure.Repositories;

namespace WholesaleManager.Infrastructure.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoriesRepository> _logger;
        public CategoriesRepository(ApplicationDbContext db, ILogger<CategoriesRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllCategories), nameof(CategoriesRepository));

            return await _db.Category.ToListAsync();
        }
    }
}

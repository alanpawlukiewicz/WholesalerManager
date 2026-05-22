using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CategoryDTO;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;

namespace WholesalerManager.Core.Services.CategoriesServices
{
    public class CategoriesGetterService : ICategoriesGetterService
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ILogger<CategoriesGetterService> _logger;

        public CategoriesGetterService(ICategoriesRepository categoriesRepository, ILogger<CategoriesGetterService> logger)
        {
            _categoriesRepository = categoriesRepository;
            _logger = logger;
        }

        public async Task<List<CategoryResponse>> GetAllCategories()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllCategories), nameof(CategoriesGetterService));
            var categories = await _categoriesRepository.GetAllCategories();
            return categories.Select(c => c.ToCategoryResponse()).ToList();
        }
    }
}

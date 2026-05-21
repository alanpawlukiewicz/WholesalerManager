using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.CategoryDTO;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;

namespace WholesalerManager.Core.Services.CategoriesServices
{
    public class CategoriesGetterService : ICategoriesGetterService
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoriesGetterService(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<List<CategoryResponse>> GetAllCategories()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return categories.Select(c => c.ToCategoryResponse()).ToList();
        }
    }
}

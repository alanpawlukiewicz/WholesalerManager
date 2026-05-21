using WholesalerManager.Core.DTO.CategoryDTO;

namespace WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts
{
    /// <summary>
    /// Represents business logic for retrieving category data.
    /// </summary>
    public interface ICategoriesGetterService
    {
        /// <summary>
        /// Returns all categories.
        /// </summary>
        /// <returns>List of CategoryResponse DTO objects.</returns>
        Task<List<CategoryResponse>> GetAllCategories();
    }
}

using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represent data logic for managing categories.
    /// </summary>
    public interface ICategoriesRepository
    {
        /// <summary>
        /// Asynchronously retrieves all categories from the database.
        /// </summary>
        /// <returns></returns>
        Task<List<Category>> GetAllCategories();
    }
}

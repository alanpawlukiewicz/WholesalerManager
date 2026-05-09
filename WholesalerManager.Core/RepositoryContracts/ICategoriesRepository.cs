using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
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

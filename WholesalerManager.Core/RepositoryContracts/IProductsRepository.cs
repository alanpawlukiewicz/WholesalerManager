using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
{
    public interface IProductsRepository
    {
        /// <summary>
        /// Asynchronously retrieves all products from the database.
        /// </summary>
        /// <returns>A list of all products.</returns>
        Task<List<Product>> GetAllProducts();

        /// <summary>
        /// Asynchronously retrieves a product by its unique identifier (ProductID).
        /// </summary>
        /// <param name="productID">The unique identifier of the product.</param>
        /// <returns>The product with the specified ProductID, or null if not found.</returns>
        Task<Product?> GetProductById(Guid productID);
    }
}

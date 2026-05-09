using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
{
    /// <summary>
    /// Represent data logic for managing products.
    /// </summary>
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

        /// <summary>
        /// Asynchronously updates an existing product in the database.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Product> UpdateProduct(Product product);


        /// <summary>
        /// Asynchronously adds a new product to the database.
        /// </summary>
        /// <param name="product">The product to be added.</param>
        /// <returns>The added product.</returns>
        Task<Product> AddProduct(Product product);

        /// <summary>
        /// Asynchronously deletes a product from the database based on its unique identifier (ProductID).
        /// </summary>
        /// <param name="productID">The unique identifier of the product to be deleted.</param>
        /// <returns>True if the product was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteProduct(Guid productID);
    }
}

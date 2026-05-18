using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.CustomerDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts.ProductServiceContracts
{
    /// <summary>
    /// Represent buisness logic for getting products from the system. This service defines the contract for retrieving products, which includes methods for fetching all products and fetching a product by its unique identifier (ProductID).
    /// </summary>
    public interface IProductsGetterService
    {
        /// <summary>
        /// Returns all products.
        /// </summary>
        /// <returns></returns>
        Task<List<ProductResponse>> GetAllProducts();

        /// <summary>
        /// Returns product with given id.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
         Task<ProductResponse?> GetProductById(Guid? productID);

        /// <summary>
        /// Asynchronously retrieves a list of products that match the specified filter criteria. The method allows filtering based on a specific property of the product, with an option to ignore case sensitivity during the filtering process.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter by.</param>
        /// <param name="filter">The filter value.</param>
        /// <param name="ignoreCase">Indicates whether to ignore case sensitivity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="ProductResponse"/> objects representing the filtered products.</returns>
        Task<List<ProductResponse>> GetFilteredProducts(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of products sorted based on a specified property and sort order. The method allows sorting by a specific property of the product, with an option to specify the sort order (ascending or descending).
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="sortOrder">The sort order (ascending or descending).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="ProductResponse"/> objects representing the sorted products.</returns>
        Task<List<ProductResponse>> GetSortedProducts(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

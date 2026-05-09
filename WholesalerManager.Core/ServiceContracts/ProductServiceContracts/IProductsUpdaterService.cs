using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.ProductDTO;

namespace WholesalerManager.Core.ServiceContracts.ProductServiceContracts
{
    /// <summary>
    /// Represents business logic for updating existing products in the system. This service defines the contract for updating a product, which includes validating the input data and returning the details of the updated product.
    /// </summary>
    public interface IProductsUpdaterService
    {
        /// <summary>
        /// Updates an existing product with the specified changes asynchronously.
        /// </summary>
        /// <param name="productUpdateRequest">An object containing the updated product information. If null, the update operation will not be performed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a ProductResponse with the
        /// updated product details.</returns>
        Task<ProductResponse> UpdateProduct(ProductUpdateRequest? productUpdateRequest);
    }
}

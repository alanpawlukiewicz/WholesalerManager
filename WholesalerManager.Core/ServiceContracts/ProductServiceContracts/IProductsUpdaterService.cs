using WholesalerManager.Core.DTO;
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


        /// <summary>
        /// Updates unit price of a product asynchronously. The method takes an EditUnitPriceDTO object as input, which contains the product ID and the new unit price. If the input DTO is null, the method will not perform any update and will return false. Otherwise, it will update the unit price of the specified product and return true if the update was successful.
        /// </summary>
        /// <param name="editUnitPriceDTO"></param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateUnitPrice(EditUnitPriceDTO? editUnitPriceDTO);

        /// <summary>
        /// Updates the stock quantity of a product asynchronously. The method takes an EditStockQuantityDTO object as input, which contains the product ID and the new stock quantity. If the input DTO is null, the method will not perform any update and will return false. Otherwise, it will update the stock quantity of the specified product and return true if the update was successful.
        /// </summary>
        /// <param name="editStockQuantityDTO"></param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateStockQuantity(EditStockQuantityDTO? editStockQuantityDTO);
    }
}

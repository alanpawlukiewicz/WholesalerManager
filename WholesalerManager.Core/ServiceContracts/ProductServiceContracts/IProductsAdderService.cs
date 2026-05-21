using WholesalerManager.Core.DTO.ProductDTO;

namespace WholesalerManager.Core.ServiceContracts.ProductServiceContracts
{
    /// <summary>
    /// Represents buisiness logic for adding new products to the system. This service defines the contract for adding a product, which includes validating the input data and returning the details of the added product.
    /// </summary>
    public interface IProductsAdderService
    {
        /// <summary>
        /// Adds a new product to the system based on the provided ProductAddRequest.
        /// </summary>
        /// <param name="productAddRequest">The request DTO object containing the details of the product to be added.</param>
        /// <returns>A ProductResponse DTO object containing the details of the added product.</returns>
        Task<ProductResponse> AddProduct(ProductAddRequest? productAddRequest);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsUpdaterService : IProductsUpdaterService
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsUpdaterService(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<ProductResponse> UpdateProduct(ProductUpdateRequest? productUpdateRequest)
        {
            if (productUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(productUpdateRequest));
            }

            ValidationHelper.ModelValidation(productUpdateRequest);

            Product? matchingProduct = await _productsRepository.GetProductById(productUpdateRequest.ProductID);

            if (matchingProduct is null)
            {
                throw new ArgumentException(nameof(matchingProduct));
            }

            Product updatedProduct = productUpdateRequest.ToProduct();

            await _productsRepository.UpdateProduct(updatedProduct);

            return updatedProduct.ToProductResponse();
        }
    }
}

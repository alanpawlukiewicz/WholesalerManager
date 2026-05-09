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
    public class ProductsAdderService : IProductsAdderService
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsAdderService(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<ProductResponse> AddProduct(ProductAddRequest? productAddRequest)
        {
            if (productAddRequest is null)
            {
                throw new ArgumentNullException(nameof(productAddRequest));
            }

            ValidationHelper.ModelValidation(productAddRequest);

            Product product = productAddRequest.ToProduct();
            product.ProductID = Guid.NewGuid();

            await _productsRepository.AddProduct(product);

            return product.ToProductResponse();
        }
    }
}

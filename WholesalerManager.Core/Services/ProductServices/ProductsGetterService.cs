using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsGetterService : IProductsGetterService
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsGetterService(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<List<ProductResponse>> GetAllProducts()
        {
            var products = await _productsRepository.GetAllProducts();
            return products.Select(product => product.ToProductResponse()).ToList();
        }

        public async Task<ProductResponse?> GetProductById(Guid? productID)
        {
            if (productID is null)
            {
                return null;
            }

            Product? foundProduct = await _productsRepository.GetProductById(productID.Value);

            if (foundProduct is null)
            {
                return null;
            }

            return foundProduct.ToProductResponse();
        }
    }
}

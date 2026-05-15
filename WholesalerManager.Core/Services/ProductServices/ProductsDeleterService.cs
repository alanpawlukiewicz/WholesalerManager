using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsDeleterService : IProductsDeleterService
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsDeleterService(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<bool> DeleteProduct(Guid? productID)
        {
            if (productID is null)
            {
                throw new ArgumentNullException(nameof(productID));
            }

            bool isDeleted = await _productsRepository.DeleteProduct(productID.Value);

            return isDeleted;
        }
    }
}

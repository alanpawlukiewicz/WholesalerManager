using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsDeleterService : IProductsDeleterService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<ProductsDeleterService> _logger;

        public ProductsDeleterService(IProductsRepository productsRepository, ILogger<ProductsDeleterService> logger)
        {
            _productsRepository = productsRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteProduct(Guid? productID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteProduct), nameof(ProductsDeleterService));
            if (productID is null)
            {
                _logger.LogError("{id} from {methodName} from {serviceName} is null.", nameof(productID), nameof(DeleteProduct), nameof(ProductsDeleterService));
                throw new ArgumentNullException(nameof(productID));
            }

            bool isDeleted = await _productsRepository.DeleteProduct(productID.Value);

            return isDeleted;
        }
    }
}

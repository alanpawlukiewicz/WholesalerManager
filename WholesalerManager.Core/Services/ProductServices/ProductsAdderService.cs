using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsAdderService : IProductsAdderService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<ProductsAdderService> _logger;

        public ProductsAdderService(IProductsRepository productsRepository, ILogger<ProductsAdderService> logger)
        {
            _productsRepository = productsRepository;
            _logger = logger;
        }

        public async Task<ProductResponse> AddProduct(ProductAddRequest? productAddRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddProduct), nameof(ProductsAdderService));

            if (productAddRequest is null)
            {
                _logger.LogError("{addRequest} from {methodName} from {serviceName} is null.", nameof(productAddRequest), nameof(AddProduct), nameof(ProductsAdderService));
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

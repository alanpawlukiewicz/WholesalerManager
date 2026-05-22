using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsUpdaterService : IProductsUpdaterService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<ProductsUpdaterService> _logger;

        public ProductsUpdaterService(IProductsRepository productsRepository, ILogger<ProductsUpdaterService> logger)
        {
            _productsRepository = productsRepository;
            _logger = logger;
        }

        public async Task<ProductResponse> UpdateProduct(ProductUpdateRequest? productUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateProduct), nameof(ProductsUpdaterService));

            if (productUpdateRequest is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(productUpdateRequest), nameof(UpdateProduct), nameof(ProductsUpdaterService));
                throw new ArgumentNullException(nameof(productUpdateRequest));
            }

            ValidationHelper.ModelValidation(productUpdateRequest);

            Product? matchingProduct = await _productsRepository.GetProductById(productUpdateRequest.ProductID);

            if (matchingProduct is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(matchingProduct), nameof(UpdateProduct), nameof(ProductsUpdaterService));
                throw new ArgumentException(nameof(matchingProduct));
            }

            Product updatedProduct = productUpdateRequest.ToProduct();

            await _productsRepository.UpdateProduct(updatedProduct);

            return updatedProduct.ToProductResponse();
        }

        public async Task<bool> UpdateStockQuantity(EditStockQuantityDTO? editStockQuantityDTO)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateStockQuantity), nameof(ProductsUpdaterService));

            if (editStockQuantityDTO is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(EditStockQuantityDTO), nameof(UpdateStockQuantity), nameof(ProductsUpdaterService));
                throw new ArgumentNullException(nameof(editStockQuantityDTO));
            }

            if (editStockQuantityDTO.ProductID == Guid.Empty || editStockQuantityDTO.NewStockQuantity < 0)
            {
                return false;
            }

            Product? matchingProduct = await _productsRepository.GetProductById(editStockQuantityDTO.ProductID);

            if (matchingProduct is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(matchingProduct), nameof(UpdateStockQuantity), nameof(ProductsUpdaterService));
                return false;
            }

            matchingProduct.StockQuantity = editStockQuantityDTO.NewStockQuantity;
            await _productsRepository.Save();

            return true;

        }

        public async Task<bool> UpdateUnitPrice(EditUnitPriceDTO? editUnitPriceDTO)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateUnitPrice), nameof(ProductsUpdaterService));

            if (editUnitPriceDTO is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(editUnitPriceDTO), nameof(UpdateUnitPrice), nameof(ProductsUpdaterService));

                throw new ArgumentNullException(nameof(editUnitPriceDTO));
            }

            decimal newUnitPriceDecimal = editUnitPriceDTO.NewUnitPrice.ToDecimalSafe();
            if (newUnitPriceDecimal <= 0 || editUnitPriceDTO.ProductID == Guid.Empty)
            {
                _logger.LogWarning("{argumentName} or {argumentName2} from {methodName} from {serviceName} has invalid data.", nameof(newUnitPriceDecimal), nameof(editUnitPriceDTO.ProductID), nameof(UpdateUnitPrice), nameof(ProductsUpdaterService));
                return false;
            }

            Product? matchingProduct = await _productsRepository.GetProductById(editUnitPriceDTO.ProductID);

            if (matchingProduct is null)
            {
                _logger.LogError("{argumentName} from {methodName} from {serviceName} is null.", nameof(matchingProduct), nameof(UpdateUnitPrice), nameof(ProductsUpdaterService));
                return false;
            }

            matchingProduct.UnitPrice = newUnitPriceDecimal;
            await _productsRepository.Save();

            return true;

        }
    }
}

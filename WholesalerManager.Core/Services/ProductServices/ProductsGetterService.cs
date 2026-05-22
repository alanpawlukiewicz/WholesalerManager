using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;

namespace WholesalerManager.Core.Services.ProductServices
{
    public class ProductsGetterService : IProductsGetterService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<ProductsGetterService> _logger;
        public ProductsGetterService(IProductsRepository productsRepository, ILogger<ProductsGetterService> logger)
        {
            _productsRepository = productsRepository;
            _logger = logger;
        }

        public async Task<List<ProductResponse>> GetAllProducts()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllProducts), nameof(ProductsGetterService));

            var products = await _productsRepository.GetAllProducts();
            return products.Select(product => product.ToProductResponse()).ToList();
        }

        public async Task<List<ProductResponse>> GetFilteredProducts(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredProducts), nameof(ProductsGetterService));

            var allProducts = await _productsRepository.GetAllProducts();
            var productResponses = allProducts.Select(p => p.ToProductResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                _logger.LogInformation("{methodName} from {serviceName} returning all products.", nameof(GetFilteredProducts), nameof(ProductsGetterService));
                return productResponses;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(ProductResponse.ProductName):
                    return productResponses.Where(p => p.ProductName != null && p.ProductName.Contains(filter, stringComparisonType)).ToList();
                case nameof(ProductResponse.SKU):
                    return productResponses.Where(p => p.SKU != null && p.SKU.Contains(filter, stringComparisonType)).ToList();
                case nameof(ProductResponse.ProductDescription):
                    return productResponses.Where(p => p.ProductDescription != null && p.ProductDescription.Contains(filter, stringComparisonType)).ToList();
                case nameof(ProductResponse.CategoryName):
                    return productResponses.Where(p => p.CategoryName != null && p.CategoryName.Contains(filter, stringComparisonType)).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }


        }

        public async Task<ProductResponse?> GetProductById(Guid? productID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked with productID: {productID}.", nameof(GetProductById), nameof(ProductsGetterService), productID);

            if (productID is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(productID), nameof(GetProductById), nameof(ProductsGetterService));
                throw new ArgumentNullException(nameof(productID));
            }

            Product? foundProduct = await _productsRepository.GetProductById(productID.Value);

            return foundProduct?.ToProductResponse();
        }

        public async Task<List<ProductResponse>> GetSortedProducts(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedProducts), nameof(ProductsGetterService));

            var allProducts = await _productsRepository.GetAllProducts();
            var productResponses = allProducts.Select(p => p.ToProductResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return productResponses;
            }

            switch (propertyName)
            {
                case nameof(ProductResponse.ProductName):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.ProductName).ToList()
                        : productResponses.OrderByDescending(p => p.ProductName).ToList();
                case nameof(ProductResponse.SKU):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.SKU).ToList()
                        : productResponses.OrderByDescending(p => p.SKU).ToList();
                case nameof(ProductResponse.ProductDescription):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.ProductDescription).ToList()
                        : productResponses.OrderByDescending(p => p.ProductDescription).ToList();
                case nameof(ProductResponse.CategoryName):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.CategoryName).ToList()
                        : productResponses.OrderByDescending(p => p.CategoryName).ToList();
                case nameof(ProductResponse.UnitPrice):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.UnitPrice).ToList()
                        : productResponses.OrderByDescending(p => p.UnitPrice).ToList();
                case nameof(ProductResponse.StockQuantity):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.StockQuantity).ToList()
                        : productResponses.OrderByDescending(p => p.StockQuantity).ToList();
                case nameof(ProductResponse.ReorderLevel):
                    return sortOrder == SortOrderOptions.ASC
                        ? productResponses.OrderBy(p => p.ReorderLevel).ToList()
                        : productResponses.OrderByDescending(p => p.ReorderLevel).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }
    }
}

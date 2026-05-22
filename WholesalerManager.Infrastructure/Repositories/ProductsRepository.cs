using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesaleManager.Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductsRepository> _logger;

        public ProductsRepository(ApplicationDbContext db, ILogger<ProductsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Product> AddProduct(Product product)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddProduct), nameof(ProductsRepository));

            await _db.Product.AddAsync(product);
            await Save();
            return product;
        }

        public async Task<bool> DeleteProduct(Guid productID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked with productID: {productID}.", nameof(DeleteProduct), nameof(ProductsRepository), productID);

            _db.Product.RemoveRange(_db.Product.Where(product => product.ProductID == productID));
            int rowsAffected = await Save();
            return rowsAffected > 0;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllProducts), nameof(ProductsRepository));

            return await _db.Product.Include("Category").ToListAsync();
        }

        public async Task<Product?> GetProductById(Guid productID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked with productID: {productID}.", nameof(GetProductById), nameof(ProductsRepository), productID);

            return await _db.Product.Include("Category").FirstOrDefaultAsync(p => p.ProductID == productID);
        }

        public async Task<int> Save()
        {
            return await _db.SaveChangesAsync();
        }

        public async Task<Product?> UpdateProduct(Product product)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked with productID: {productID}.", nameof(UpdateProduct), nameof(ProductsRepository), product.ProductID);

            Product? matchingProduct = await _db.Product.FirstOrDefaultAsync(temp => temp.ProductID == product.ProductID);

            if (matchingProduct is null)
            {
                _logger.LogWarning("Product not found.");
                return null;
            }

            matchingProduct.ProductName = product.ProductName;
            matchingProduct.SKU = product.SKU;
            matchingProduct.ProductDescription = product.ProductDescription;
            matchingProduct.CategoryID = product.CategoryID;
            matchingProduct.UnitPrice = product.UnitPrice;
            matchingProduct.StockQuantity = product.StockQuantity;
            matchingProduct.ReorderLevel = product.ReorderLevel;

            await Save();

            return matchingProduct;
        }
    }
}

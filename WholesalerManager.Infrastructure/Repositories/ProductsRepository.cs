using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesaleManager.Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Product> AddProduct(Product product)
        {
            await _db.Product.AddAsync(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProduct(Guid productID)
        {
            _db.Product.RemoveRange(_db.Product.Where(product => product.ProductID == productID));
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _db.Product.Include("Category").ToListAsync();
        }

        public async Task<Product?> GetProductById(Guid productID)
        {
            return await _db.Product.Include("Category").FirstOrDefaultAsync(p => p.ProductID == productID);
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            Product? matchingProduct = await _db.Product.FirstOrDefaultAsync(temp => temp.ProductID == product.ProductID);

            if (matchingProduct is null)
            {
                return product;
            }

            matchingProduct.ProductName = product.ProductName;
            matchingProduct.SKU = product.SKU;
            matchingProduct.ProductDescription = product.ProductDescription;
            matchingProduct.CategoryID = product.CategoryID;
            matchingProduct.UnitPrice = product.UnitPrice;
            matchingProduct.StockQuantity = product.StockQuantity;
            matchingProduct.ReorderLevel = product.ReorderLevel;

            await _db.SaveChangesAsync();

            return matchingProduct;
        }
    }
}

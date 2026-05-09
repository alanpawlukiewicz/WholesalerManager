using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;

namespace WholesaleManager.Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductsRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<List<Product>> GetAllProducts()
        {
            return await _db.Product.Include("Category").ToListAsync();
        }

        public async Task<Product?> GetProductById(Guid productID)
        {
            return await _db.Product.Include("Category").FirstOrDefaultAsync(p => p.ProductID == productID);
        }
    }
}

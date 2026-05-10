using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class DeliveryItemsRepository : IDeliveryItemsRepository
    {
        private readonly ApplicationDbContext _db;

        public DeliveryItemsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeliveryItem> AddDeliveryItem(DeliveryItem item)
        {
            await _db.DeliveryItem.AddAsync(item);
            await _db.SaveChangesAsync();

            return item;
        }

        public async Task<List<DeliveryItem>> AddMultipleDeliveryItems(List<DeliveryItem> items)
        {
            await _db.DeliveryItem.AddRangeAsync(items);
            await _db.SaveChangesAsync();

            return items;
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItems()
        {
            return await _db.DeliveryItem.Include("Product").Include("Delivery").ToListAsync();
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItemsFromDelivery(Guid deliveryID)
        {
            return await _db.DeliveryItem.Include("Product").Include("Delivery").Where(i => i.DeliveryID == deliveryID).ToListAsync();
        }
    }
}

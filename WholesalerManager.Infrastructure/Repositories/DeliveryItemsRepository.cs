using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

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

        public async Task<bool> DeleteDeliveryItem(DeliveryItem item)
        {
            _db.DeliveryItem.Remove(item);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItems()
        {
            return await _db.DeliveryItem.Include("Product").Include("Delivery").ToListAsync();
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItemsFromDelivery(Guid deliveryID)
        {
            return await _db.DeliveryItem.Include("Product").Include("Delivery").Where(i => i.DeliveryID == deliveryID).ToListAsync();
        }

        public async Task<DeliveryItem?> UpdateDeliveryItem(DeliveryItem item)
        {
            DeliveryItem? matchingItem = await _db.DeliveryItem.FirstOrDefaultAsync(i => i.DeliveryItemID == item.DeliveryItemID);

            if (matchingItem is null)
            {
                return null;
            }

            matchingItem.DeliveryID = item.DeliveryID;
            matchingItem.ProductID = item.ProductID;
            matchingItem.Quantity = item.Quantity;
            matchingItem.PriceAtSale = item.PriceAtSale;

            await _db.SaveChangesAsync();

            return matchingItem;
        }

        public async Task<List<DeliveryItem?>> UpdateMultipleDeliveryItems(List<DeliveryItem> items)
        {
            List<DeliveryItem?> updatedItems = new List<DeliveryItem?>() { };
            foreach(var item in items)
            {
                updatedItems.Add(await UpdateDeliveryItem(item));
            }

            return updatedItems;
        }
    }
}

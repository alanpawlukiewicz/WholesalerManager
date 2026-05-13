using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class OrderItemsRepository : IOrderItemsRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderItemsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OrderItem> AddOrderItem(OrderItem item)
        {
            await _db.OrderItem.AddAsync(item);
            await _db.SaveChangesAsync();

            return item;
        }

        public async Task<List<OrderItem>> AddMultipleOrderItems(List<OrderItem> items)
        {
            await _db.OrderItem.AddRangeAsync(items);
            await _db.SaveChangesAsync();

            return items;
        }

        public async Task<bool> DeleteOrderItem(OrderItem item)
        {
            _db.OrderItem.Remove(item);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<OrderItem>> GetAllOrderItems()
        {
            return await _db.OrderItem.Include("Product").Include("Order").ToListAsync();
        }

        public async Task<List<OrderItem>> GetAllOrderItemsFromOrder(Guid orderID)
        {
            return await _db.OrderItem.Include("Product").Include("Order").Where(i => i.OrderID == orderID).ToListAsync();
        }

        public async Task<OrderItem?> UpdateOrderItem(OrderItem item)
        {
            OrderItem? matchingItem = await _db.OrderItem.FirstOrDefaultAsync(i => i.OrderItemID == item.OrderItemID);

            if (matchingItem is null)
            {
                return null;
            }

            matchingItem.OrderID = item.OrderID;
            matchingItem.ProductID = item.ProductID;
            matchingItem.Quantity = item.Quantity;
            matchingItem.PriceAtSale = item.PriceAtSale;

            await _db.SaveChangesAsync();

            return matchingItem;
        }

        public async Task<List<OrderItem?>> UpdateMultipleOrderItems(List<OrderItem> items)
        {
            List<OrderItem?> updatedItems = new List<OrderItem?>() { };
            foreach (var item in items)
            {
                updatedItems.Add(await UpdateOrderItem(item));
            }

            return updatedItems;
        }
    }
}

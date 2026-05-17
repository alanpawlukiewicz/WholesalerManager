using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly ApplicationDbContext _db;

        public OrdersRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _db.Order.Include("Customer").ToListAsync();
        }

        public async Task<Order?> GetOrderByID(Guid orderID)
        {
            return await _db.Order.Include("Customer").FirstOrDefaultAsync(o => o.OrderID == orderID);
        }


        public async Task<bool> DeleteOrderById(Guid orderID)
        {
            _db.Order.RemoveRange(_db.Order.Where(o => o.OrderID == orderID));
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            Order? matchingOrder = await _db.Order.FirstOrDefaultAsync(o => o.OrderID == order.OrderID);

            if (matchingOrder is null)
            {
                return null;
            }

            matchingOrder.OrderDate = order.OrderDate;
            matchingOrder.Status = order.Status;
            matchingOrder.CustomerID = order.CustomerID;


            return matchingOrder;
        }

        public async Task<Order> AddOrder(Order order)
        {
            await _db.Order.AddAsync(order);
            return order;
        }

        public async Task<int> Save()
        {
            return await _db.SaveChangesAsync();
        }
    }
}

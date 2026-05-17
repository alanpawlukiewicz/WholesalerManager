using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class DeliveriesRepository : IDeliveriesRepository
    {
        private readonly ApplicationDbContext _db;

        public DeliveriesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Delivery> AddDelivery(Delivery delivery)
        {
            await _db.Delivery.AddAsync(delivery);
            return delivery;
        }

        public async Task<bool> DeleteDeliveryById(Guid deliveryID)
        {
            _db.Delivery.RemoveRange(_db.Delivery.Where(d => d.DeliveryID == deliveryID));
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Delivery>> GetAllDeliveries()
        {
            return await _db.Delivery.Include("Supplier").ToListAsync();
        }

        public async Task<Delivery?> GetDeliveryById(Guid deliveryID)
        {
            return await _db.Delivery.Include("Supplier").FirstOrDefaultAsync(d => d.DeliveryID == deliveryID);
        }

        public async Task<Delivery?> UpdateDelivery(Delivery delivery)
        {
            Delivery? matchingDelivery = await _db.Delivery.FirstOrDefaultAsync(d => d.DeliveryID == delivery.DeliveryID);

            if (matchingDelivery is null)
            {
                return null;
            }

            matchingDelivery.OrderDate = delivery.OrderDate;
            matchingDelivery.Status = delivery.Status;
            matchingDelivery.SupplierID = delivery.SupplierID;

            return matchingDelivery;
        }
    }
}

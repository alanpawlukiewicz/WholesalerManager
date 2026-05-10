using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.RepositoryContracts;

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
            await _db.SaveChangesAsync();
            return delivery;
        }

        public async Task<List<Delivery>> GetAllDeliveries()
        {
            return await _db.Delivery.Include("Supplier").ToListAsync();
        }

        public async Task<Delivery?> GetDeliveryById(Guid deliveryID)
        {
            return await _db.Delivery.Include("Supplier").FirstOrDefaultAsync(d => d.DeliveryID == deliveryID);
        }
    }
}

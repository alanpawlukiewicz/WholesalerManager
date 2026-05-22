using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class DeliveriesRepository : IDeliveriesRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DeliveriesRepository> _logger;

        public DeliveriesRepository(ApplicationDbContext db, ILogger<DeliveriesRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Delivery> AddDelivery(Delivery delivery)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddDelivery), nameof(DeliveriesRepository));

            await _db.Delivery.AddAsync(delivery);
            return delivery;
        }

        public async Task<bool> DeleteDeliveryById(Guid deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteDeliveryById), nameof(DeliveriesRepository));

            _db.Delivery.RemoveRange(_db.Delivery.Where(d => d.DeliveryID == deliveryID));
            int rowsAffected = await Save();
            return rowsAffected > 0;
        }

        public async Task<List<Delivery>> GetAllDeliveries()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveries), nameof(DeliveriesRepository));

            return await _db.Delivery.Include("Supplier").ToListAsync();
        }

        public async Task<Delivery?> GetDeliveryById(Guid deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetDeliveryById), nameof(DeliveriesRepository));

            return await _db.Delivery.Include("Supplier").FirstOrDefaultAsync(d => d.DeliveryID == deliveryID);
        }

        public async Task<int> Save()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(Save), nameof(DeliveriesRepository));

            return await _db.SaveChangesAsync();
        }

        public async Task<Delivery?> UpdateDelivery(Delivery delivery)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateDelivery), nameof(DeliveriesRepository));

            Delivery? matchingDelivery = await _db.Delivery.FirstOrDefaultAsync(d => d.DeliveryID == delivery.DeliveryID);

            if (matchingDelivery is null)
            {
                _logger.LogWarning("Delivery not found.");
                return null;
            }

            matchingDelivery.OrderDate = delivery.OrderDate;
            matchingDelivery.Status = delivery.Status;
            matchingDelivery.SupplierID = delivery.SupplierID;

            return matchingDelivery;
        }
    }
}

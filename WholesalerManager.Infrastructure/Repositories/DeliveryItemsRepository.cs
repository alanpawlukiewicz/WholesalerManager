using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class DeliveryItemsRepository : IDeliveryItemsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DeliveryItemsRepository> _logger;

        public DeliveryItemsRepository(ApplicationDbContext db, ILogger<DeliveryItemsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<DeliveryItem> AddDeliveryItem(DeliveryItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddDeliveryItem), nameof(DeliveryItemsRepository));

            await _db.DeliveryItem.AddAsync(item);
            return item;
        }

        public async Task<List<DeliveryItem>> AddMultipleDeliveryItems(List<DeliveryItem> items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddMultipleDeliveryItems), nameof(DeliveryItemsRepository));

            await _db.DeliveryItem.AddRangeAsync(items);
            return items;
        }

        public async Task<bool> DeleteDeliveryItem(DeliveryItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteDeliveryItem), nameof(DeliveryItemsRepository));

            _db.DeliveryItem.Remove(item);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItems()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveryItems), nameof(DeliveryItemsRepository));

            return await _db.DeliveryItem.Include("Product").Include("Delivery").ToListAsync();
        }

        public async Task<List<DeliveryItem>> GetAllDeliveryItemsFromDelivery(Guid deliveryID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllDeliveryItemsFromDelivery), nameof(DeliveryItemsRepository));

            return await _db.DeliveryItem.Include("Product").Include("Delivery").Where(i => i.DeliveryID == deliveryID).ToListAsync();
        }

        public async Task<DeliveryItem?> UpdateDeliveryItem(DeliveryItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateDeliveryItem), nameof(DeliveryItemsRepository));

            DeliveryItem? matchingItem = await _db.DeliveryItem.FirstOrDefaultAsync(i => i.DeliveryItemID == item.DeliveryItemID);

            if (matchingItem is null)
            {
                _logger.LogWarning("Delivery item not found.");
                return null;
            }

            matchingItem.DeliveryID = item.DeliveryID;
            matchingItem.ProductID = item.ProductID;
            matchingItem.Quantity = item.Quantity;
            matchingItem.PriceAtSale = item.PriceAtSale;

            return matchingItem;
        }

        public async Task<List<DeliveryItem?>> UpdateMultipleDeliveryItems(List<DeliveryItem> items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateMultipleDeliveryItems), nameof(DeliveryItemsRepository));

            List<DeliveryItem?> updatedItems = new List<DeliveryItem?>() { };
            foreach (var item in items)
            {
                updatedItems.Add(await UpdateDeliveryItem(item));
            }

            return updatedItems;
        }
    }
}

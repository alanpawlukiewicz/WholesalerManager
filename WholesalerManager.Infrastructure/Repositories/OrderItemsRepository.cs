using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class OrderItemsRepository : IOrderItemsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrderItemsRepository> _logger;

        public OrderItemsRepository(ApplicationDbContext db, ILogger<OrderItemsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<OrderItem> AddOrderItem(OrderItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddOrderItem), nameof(OrderItemsRepository));

            await _db.OrderItem.AddAsync(item);
            return item;
        }

        public async Task<List<OrderItem>> AddMultipleOrderItems(List<OrderItem> items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddMultipleOrderItems), nameof(OrderItemsRepository));

            await _db.OrderItem.AddRangeAsync(items);
            return items;
        }

        public async Task<bool> DeleteOrderItem(OrderItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteOrderItem), nameof(OrderItemsRepository));

            _db.OrderItem.Remove(item);
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<List<OrderItem>> GetAllOrderItems()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllOrderItems), nameof(OrderItemsRepository));

            return await _db.OrderItem.Include("Product").Include("Order").ToListAsync();
        }

        public async Task<List<OrderItem>> GetAllOrderItemsFromOrder(Guid orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllOrderItemsFromOrder), nameof(OrderItemsRepository));

            return await _db.OrderItem.Include("Product").Include("Order").Where(i => i.OrderID == orderID).ToListAsync();
        }

        public async Task<OrderItem?> UpdateOrderItem(OrderItem item)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrderItem), nameof(OrderItemsRepository));

            OrderItem? matchingItem = await _db.OrderItem.FirstOrDefaultAsync(i => i.OrderItemID == item.OrderItemID);

            if (matchingItem is null)
            {
                _logger.LogWarning("Order item not found.");
                return null;
            }

            matchingItem.OrderID = item.OrderID;
            matchingItem.ProductID = item.ProductID;
            matchingItem.Quantity = item.Quantity;
            matchingItem.PriceAtSale = item.PriceAtSale;

            return matchingItem;
        }

        public async Task<List<OrderItem?>> UpdateMultipleOrderItems(List<OrderItem> items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateMultipleOrderItems), nameof(OrderItemsRepository));

            List<OrderItem?> updatedItems = new List<OrderItem?>() { };
            foreach (var item in items)
            {
                updatedItems.Add(await UpdateOrderItem(item));
            }

            return updatedItems;
        }
    }
}

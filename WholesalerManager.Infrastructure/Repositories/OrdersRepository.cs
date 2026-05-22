using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Infrastructure.DatabaseContext;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OrdersRepository> _logger;

        public OrdersRepository(ApplicationDbContext db, ILogger<OrdersRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllOrders), nameof(OrdersRepository));

            return await _db.Order.Include("Customer").ToListAsync();
        }

        public async Task<Order?> GetOrderByID(Guid orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetOrderByID), nameof(OrdersRepository));

            return await _db.Order.Include("Customer").FirstOrDefaultAsync(o => o.OrderID == orderID);
        }


        public async Task<bool> DeleteOrderById(Guid orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteOrderById), nameof(OrdersRepository));

            _db.Order.RemoveRange(_db.Order.Where(o => o.OrderID == orderID));
            int rowsAffected = await _db.SaveChangesAsync();
            return rowsAffected > 0;
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrder), nameof(OrdersRepository));

            Order? matchingOrder = await _db.Order.FirstOrDefaultAsync(o => o.OrderID == order.OrderID);

            if (matchingOrder is null)
            {
                _logger.LogWarning("Order not found.");
                return null;
            }

            matchingOrder.OrderDate = order.OrderDate;
            matchingOrder.Status = order.Status;
            matchingOrder.CustomerID = order.CustomerID;


            return matchingOrder;
        }

        public async Task<Order> AddOrder(Order order)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddOrder), nameof(OrdersRepository));

            await _db.Order.AddAsync(order);
            return order;
        }

        public async Task<int> Save()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(Save), nameof(OrdersRepository));

            return await _db.SaveChangesAsync();
        }
    }
}

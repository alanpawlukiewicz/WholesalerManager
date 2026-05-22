using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class OrdersDeleterService : IOrdersDeleterService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILogger<OrdersDeleterService> _logger;

        public OrdersDeleterService(IOrdersRepository ordersRepository, ILogger<OrdersDeleterService> logger)
        {
            _ordersRepository = ordersRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteOrderByID(Guid? orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteOrderByID), nameof(OrdersDeleterService));
            if (orderID is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(orderID), nameof(DeleteOrderByID), nameof(OrdersDeleterService));
                throw new ArgumentNullException(nameof(orderID));
            }

            if (orderID == Guid.Empty)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is empty.", nameof(orderID), nameof(DeleteOrderByID), nameof(OrdersDeleterService));
                throw new ArgumentException(nameof(orderID));
            }

            return await _ordersRepository.DeleteOrderById(orderID.Value);
        }
    }
}

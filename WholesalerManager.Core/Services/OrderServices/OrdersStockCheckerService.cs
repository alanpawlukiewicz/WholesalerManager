using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersStockCheckerService : IOrdersStockCheckerService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IOrderItemsRepository _itemsRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ILogger<OrdersStockCheckerService> _logger;

        public OrdersStockCheckerService(IOrdersRepository ordersRepository, IOrderItemsRepository itemsRepository, IProductsRepository productsRepository, ILogger<OrdersStockCheckerService> logger)
        {
            _ordersRepository = ordersRepository;
            _itemsRepository = itemsRepository;
            _productsRepository = productsRepository;
            _logger = logger;
        }

        public async Task<bool> CheckStockAvailabilityForOrder(Guid? orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));

            if (orderID is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(orderID), nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));
                throw new ArgumentNullException(nameof(orderID));
            }
            Order? matchingOrder = await _ordersRepository.GetOrderByID(orderID.Value);

            if (matchingOrder is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(matchingOrder), nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));
                throw new ArgumentNullException(nameof(matchingOrder));
            }

            var itemsFromOrder = await _itemsRepository.GetAllOrderItemsFromOrder(orderID.Value);

            if (itemsFromOrder is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(itemsFromOrder), nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));
                throw new ArgumentNullException(nameof(itemsFromOrder));
            }

            foreach (var item in itemsFromOrder)
            {
                if (item.ProductID is null)
                {
                    _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(item.ProductID), nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));
                    throw new ArgumentNullException(nameof(itemsFromOrder));
                }

                var matchingProduct = await _productsRepository.GetProductById(item.ProductID.Value);

                if (matchingProduct is null)
                {
                    _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(matchingProduct), nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));
                    throw new ArgumentNullException(nameof(matchingProduct));
                }

                if (matchingProduct.StockQuantity < item.Quantity)
                {
                    _logger.LogWarning("{methodName} from {serviceName}: {variableName} is greater than {variableName2}. Returning false.", nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService), nameof(item.Quantity), nameof(matchingProduct.StockQuantity));
                    return false;
                }
            }

            _logger.LogInformation("{methodName} from {serviceName} returned true.", nameof(CheckStockAvailabilityForOrder), nameof(OrdersStockCheckerService));

            return true;
        }
    }
}

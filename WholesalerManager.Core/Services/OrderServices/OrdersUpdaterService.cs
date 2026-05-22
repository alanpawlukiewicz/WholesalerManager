using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersUpdaterService : IOrdersUpdaterService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IOrdersStockCheckerService _ordersStockCheckerService;
        private readonly ILogger<OrdersUpdaterService> _logger;

        public OrdersUpdaterService(IOrdersRepository ordersRepository, IOrdersStockCheckerService ordersStockCheckerService, ILogger<OrdersUpdaterService> logger)
        {
            _ordersRepository = ordersRepository;
            _ordersStockCheckerService = ordersStockCheckerService;
            _logger = logger;
        }

        public async Task<bool> CancelOrder(Guid orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(CancelOrder), nameof(OrdersUpdaterService));
            if (orderID == Guid.Empty)
            {
                throw new ArgumentException(nameof(orderID));
            }
            var matchingOrder = await _ordersRepository.GetOrderByID(orderID);
            if (matchingOrder is null)
            {
                return false;
            }
            if (matchingOrder.Status == OrderStatus.DELIVERED.ToString())
            {
                return false;
            }

            matchingOrder.Status = OrderStatus.CANCELLED.ToString();

            await _ordersRepository.Save();

            return true;
        }

        public async Task<OrderResponse> UpdateOrder(OrderUpdateRequest? orderUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrder), nameof(OrdersUpdaterService));

            if (orderUpdateRequest is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(orderUpdateRequest), nameof(UpdateOrderStatus), nameof(OrdersUpdaterService));
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }

            ValidationHelper.ModelValidation(orderUpdateRequest);

            // Check stock availability for the order items if order status changed to "PAID" or "PROCESSING" 
            if (orderUpdateRequest.Status == OrderStatus.PAID || orderUpdateRequest.Status == OrderStatus.PROCESSING)
            {
                bool isStockAvailable = await _ordersStockCheckerService.CheckStockAvailabilityForOrder(orderUpdateRequest.OrderID);
                if (!isStockAvailable)
                {
                    _logger.LogError("Stock is not awailvable.");
                    throw new InvalidOperationException("Insufficient stock for one or more items in the order.");
                }
            }

            Order order = orderUpdateRequest.ToOrder();

            Order? updatedOrder = await _ordersRepository.UpdateOrder(order);

            if (updatedOrder is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(updatedOrder), nameof(UpdateOrderStatus), nameof(OrdersUpdaterService));
                throw new ArgumentException(nameof(updatedOrder));
            }

            return updatedOrder.ToOrderResponse();
        }

        public async Task<bool> UpdateOrderStatus(Guid orderID, OrderStatus status)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrderStatus), nameof(OrdersUpdaterService));

            if (orderID == Guid.Empty)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is empty.", nameof(orderID), nameof(UpdateOrderStatus), nameof(OrdersUpdaterService));
                throw new ArgumentException(nameof(orderID));
            }
            var matchingOrder = await _ordersRepository.GetOrderByID(orderID);
            if (matchingOrder is null)
            {
                _logger.LogError("{variableName} from {methodName} from {serviceName} is null.", nameof(matchingOrder), nameof(UpdateOrderStatus), nameof(OrdersUpdaterService));
                return false;
            }

            if (status == OrderStatus.PENDING ||
                status == OrderStatus.DELIVERED ||
                status == OrderStatus.CANCELLED ||
                status == OrderStatus.RETURNED)
            {
                _logger.LogWarning("Invalid order status");
                return false;
            }

            if (matchingOrder.Status == OrderStatus.PENDING.ToString() && status != OrderStatus.PAID)
            {
                _logger.LogWarning("Invalid order status, expected: {statusOrder}", nameof(OrderStatus.PAID));
                return false;
            }
            if (matchingOrder.Status == OrderStatus.PAID.ToString() && status != OrderStatus.PROCESSING)
            {
                _logger.LogWarning("Invalid order status, expected: {statusOrder}", nameof(OrderStatus.PROCESSING));
                return false;
            }
            if (matchingOrder.Status == OrderStatus.PROCESSING.ToString() && status != OrderStatus.SHIPPED)
            {
                _logger.LogWarning("Invalid order status, expected: {statusOrder}", nameof(OrderStatus.SHIPPED));
                return false;
            }

            matchingOrder.Status = status.ToString();

            await _ordersRepository.Save();

            return true;
        }
    }
}

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

        public OrdersUpdaterService(IOrdersRepository ordersRepository, IOrdersStockCheckerService ordersStockCheckerService)
        {
            _ordersRepository = ordersRepository;
            _ordersStockCheckerService = ordersStockCheckerService;
        }

        public async Task<bool> CancelOrder(Guid orderID)
        {
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
            if (orderUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }

            ValidationHelper.ModelValidation(orderUpdateRequest);

            // Check stock availability for the order items if order status changed to "PAID" or "PROCESSING" 
            if (orderUpdateRequest.Status == OrderStatus.PAID || orderUpdateRequest.Status == OrderStatus.PROCESSING)
            {
                bool isStockAvailable = await _ordersStockCheckerService.CheckStockAvailabilityForOrder(orderUpdateRequest.OrderID);
                if (!isStockAvailable)
                {
                    throw new InvalidOperationException("Insufficient stock for one or more items in the order.");
                }
            }

            Order order = orderUpdateRequest.ToOrder();

            Order? updatedOrder = await _ordersRepository.UpdateOrder(order);

            if (updatedOrder is null)
            {
                throw new ArgumentException(nameof(updatedOrder));
            }

            return updatedOrder.ToOrderResponse();
        }

        public async Task<bool> UpdateOrderStatus(Guid orderID, OrderStatus status)
        {
            if (orderID == Guid.Empty)
            {
                throw new ArgumentException(nameof(orderID));
            }
            var matchingOrder = await _ordersRepository.GetOrderByID(orderID);
            if (matchingOrder is null)
            {
                return false;
            }

            if (status == OrderStatus.PENDING ||
                status == OrderStatus.DELIVERED ||
                status == OrderStatus.CANCELLED ||
                status == OrderStatus.RETURNED)
            {
                return false;
            }

            if (matchingOrder.Status == OrderStatus.PENDING.ToString() && status != OrderStatus.PAID)
            {
                return false;
            }
            if (matchingOrder.Status == OrderStatus.PAID.ToString() && status != OrderStatus.PROCESSING)
            {
                return false;
            }
            if (matchingOrder.Status == OrderStatus.PROCESSING.ToString() && status != OrderStatus.SHIPPED)
            {
                return false;
            }

            matchingOrder.Status = status.ToString();

            await _ordersRepository.Save();

            return true;
        }
    }
}

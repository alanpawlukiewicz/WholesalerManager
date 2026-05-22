using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrderUpdateCoordinatorService : IOrderUpdateCoordinatorService
    {
        private readonly IOrdersUpdaterService _ordersUpdaterService;
        private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderUpdateCoordinatorService> _logger;
        public OrderUpdateCoordinatorService(IOrdersUpdaterService ordersUpdaterService, IOrderItemsUpdaterService orderItemsUpdaterService, IUnitOfWork unitOfWork, ILogger<OrderUpdateCoordinatorService> logger)
        {
            _ordersUpdaterService = ordersUpdaterService;
            _orderItemsUpdaterService = orderItemsUpdaterService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<OrderResponse> UpdateFullOrder(OrderUpdateRequest? orderRequest, List<OrderItemUpdateRequest>? items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateFullOrder), nameof(OrderUpdateCoordinatorService));
            if (orderRequest is null || items is null)
            {
                _logger.LogError("{requestName} or {requestName2} from {methodName} from {serviceName} is null.", nameof(orderRequest), nameof(items), nameof(UpdateFullOrder), nameof(OrderUpdateCoordinatorService));
                throw new ArgumentNullException("Order request and items cannot be null.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _ordersUpdaterService.UpdateOrder(orderRequest);
                await _orderItemsUpdaterService.UpdateMultipleOrderItems(items!);

                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught from {methodName} from {serviceName}: {ex}.", nameof(UpdateFullOrder), nameof(OrderUpdateCoordinatorService), ex);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

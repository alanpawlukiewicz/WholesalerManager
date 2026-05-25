using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrderRegistrationService : IOrderRegistrationService
    {
        private readonly IOrdersAdderService _ordersAdderService;
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderRegistrationService> _logger;

        public OrderRegistrationService(IOrdersAdderService ordersAdderService, IOrderItemsAdderService orderItemsAdderService, IUnitOfWork unitOfWork, ILogger<OrderRegistrationService> logger)
        {
            _ordersAdderService = ordersAdderService;
            _orderItemsAdderService = orderItemsAdderService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OrderResponse> RegisterFullOrder(OrderAddRequest? orderRequest, List<OrderItemAddRequest>? items)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(RegisterFullOrder), nameof(OrderRegistrationService));
            if (orderRequest is null || items is null)
            {
                _logger.LogError("{requestName} or {requestName2} from {methodName} from {serviceName} is null.", nameof(orderRequest), nameof(items), nameof(RegisterFullOrder), nameof(OrderRegistrationService));
                throw new ArgumentNullException("Order request and items cannot be null.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _ordersAdderService.AddOrder(orderRequest);

                items.ForEach(i => i.OrderID = order.OrderID);

                await _orderItemsAdderService.AddMultipleOrderItems(items);

                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught from {methodName} from {serviceName}: {ex}.", nameof(RegisterFullOrder), nameof(OrderRegistrationService), ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}

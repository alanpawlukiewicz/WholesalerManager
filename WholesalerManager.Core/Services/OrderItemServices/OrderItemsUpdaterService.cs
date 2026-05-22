using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsUpdaterService : IOrderItemsUpdaterService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly ILogger<OrderItemsUpdaterService> _logger;

        public OrderItemsUpdaterService(IOrderItemsRepository orderItemsRepository, IOrderItemsAdderService orderItemsAdderService, ILogger<OrderItemsUpdaterService> logger)
        {
            _orderItemsRepository = orderItemsRepository;
            _orderItemsAdderService = orderItemsAdderService;
            _logger = logger;
        }

        public async Task<OrderItemResponse> UpdateOrderItem(OrderItemUpdateRequest? orderItemUpdateRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrderItem), nameof(OrderItemsUpdaterService));
            if (orderItemUpdateRequest is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(orderItemUpdateRequest), nameof(UpdateOrderItem), nameof(OrderItemsUpdaterService));
                throw new ArgumentNullException(nameof(orderItemUpdateRequest));
            }

            ValidationHelper.ModelValidation(orderItemUpdateRequest);

            OrderItem item = orderItemUpdateRequest.ToOrderItem();

            OrderItem? changedItem = null;
            // Check if item is being added to an existing order
            if (item.OrderItemID == Guid.Empty)
            {
                _logger.LogInformation("Adding order item to database");
                changedItem = (await _orderItemsAdderService.AddOrderItem(item.ToOrderItemAddRequest())).ToOrderItem();
            }
            else
            {
                _logger.LogInformation("Updating order item.");
                changedItem = await _orderItemsRepository.UpdateOrderItem(item);
            }

            if (changedItem is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(changedItem), nameof(UpdateOrderItem), nameof(OrderItemsUpdaterService));
                throw new ArgumentException(nameof(changedItem));
            }

            return changedItem.ToOrderItemResponse();

        }

        public async Task<List<OrderItemResponse>> UpdateMultipleOrderItems(List<OrderItemUpdateRequest?>? orderItemUpdateRequests)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateOrderItem), nameof(OrderItemsUpdaterService));
            if (orderItemUpdateRequests is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(orderItemUpdateRequests), nameof(UpdateMultipleOrderItems), nameof(OrderItemsUpdaterService));
                throw new ArgumentNullException(nameof(orderItemUpdateRequests));
            }

            List<OrderItemResponse> updatedItems = new List<OrderItemResponse>() { };

            foreach (var item in orderItemUpdateRequests)
            {
                updatedItems.Add(await UpdateOrderItem(item));
            }

            return updatedItems;
        }
    }
}

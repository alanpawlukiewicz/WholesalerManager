using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsUpdaterService : IOrderItemsUpdaterService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly IOrderItemsAdderService _orderItemsAdderService;

        public OrderItemsUpdaterService(IOrderItemsRepository orderItemsRepository, IOrderItemsAdderService orderItemsAdderService)
        {
            _orderItemsRepository = orderItemsRepository;
            _orderItemsAdderService = orderItemsAdderService;
        }

        public async Task<OrderItemResponse> UpdateOrderItem(OrderItemUpdateRequest? orderItemUpdateRequest)
        {
            if (orderItemUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(orderItemUpdateRequest));
            }

            ValidationHelper.ModelValidation(orderItemUpdateRequest);

            OrderItem item = orderItemUpdateRequest.ToOrderItem();

            OrderItem? changedItem = null;
            // Check if item is being added to an existing order
            if (item.OrderItemID == Guid.Empty)
            {
                changedItem = (await _orderItemsAdderService.AddOrderItem(item.ToOrderItemAddRequest())).ToOrderItem();
            }
            else
            {
                changedItem = await _orderItemsRepository.UpdateOrderItem(item);
            }

            if (changedItem is null)
            {
                throw new ArgumentException(nameof(changedItem));
            }

            return changedItem.ToOrderItemResponse();

        }

        public async Task<List<OrderItemResponse>> UpdateMultipleOrderItems(List<OrderItemUpdateRequest?>? orderItemUpdateRequests)
        {
            if (orderItemUpdateRequests is null)
            {
                throw new ArgumentNullException(nameof(orderItemUpdateRequests));
            }

            List<OrderItemResponse> updatedItems = new List<OrderItemResponse>() { };

            foreach(var item in orderItemUpdateRequests)
            {
                updatedItems.Add(await UpdateOrderItem(item));
            }

            return updatedItems;
        }
    }
}

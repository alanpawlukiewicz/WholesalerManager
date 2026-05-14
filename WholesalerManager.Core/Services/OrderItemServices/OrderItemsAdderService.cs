using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsAdderService : IOrderItemsAdderService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;

        public OrderItemsAdderService(IOrderItemsRepository orderItemsRepository)
        {
            _orderItemsRepository = orderItemsRepository;
        }

        public async Task<OrderItemResponse> AddOrderItem(OrderItemAddRequest? itemAddRequest)
        {
            if (itemAddRequest is null)
            {
                throw new ArgumentNullException(nameof(itemAddRequest));
            }

            ValidationHelper.ModelValidation(itemAddRequest);

            var item = itemAddRequest.ToOrderItem();
            item.OrderItemID = Guid.NewGuid();
            var addedItem = await _orderItemsRepository.AddOrderItem(item);

            return addedItem.ToOrderItemResponse();

        }

        public async Task<List<OrderItemResponse>> AddMultipleOrderItems(List<OrderItemAddRequest>? itemAddRequests)
        {
            if (itemAddRequests is null)
            {
                throw new ArgumentNullException(nameof(itemAddRequests));
            }

            foreach (var item in itemAddRequests)
            {
                ValidationHelper.ModelValidation(item);
            }

            List<OrderItemResponse> addedItems = new List<OrderItemResponse>() { };

            foreach (var item in itemAddRequests)
            {
                addedItems.Add(await AddOrderItem(item));
            }

            return addedItems;
        }
    }
}

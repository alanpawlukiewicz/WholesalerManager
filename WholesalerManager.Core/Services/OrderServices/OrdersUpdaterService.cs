using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly IOrdersGetterService _ordersGetterService;

        public OrdersUpdaterService(IOrdersRepository ordersRepository, IOrdersGetterService ordersGetterService)
        {
            _ordersRepository = ordersRepository;
            _ordersGetterService = ordersGetterService;
        }

        public async Task<bool> CancelOrder(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                throw new ArgumentException(nameof(orderID));
            }
            var matchingOrder = await _ordersGetterService.GetOrderByID(orderID);
            if (matchingOrder is null)
            {
                return false;
            }
            if (matchingOrder.Status == OrderStatus.DELIVERED.ToString())
            {
                return false;
            }

            matchingOrder.Status = OrderStatus.CANCELLED.ToString();
            var response = await _ordersRepository.UpdateOrder(matchingOrder.ToOrder());
            if (response is null)
            {
                return false;
            }

            return true;
        }

        public async Task<OrderResponse> UpdateOrder(OrderUpdateRequest? orderUpdateRequest)
        {
            if (orderUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }

            ValidationHelper.ModelValidation(orderUpdateRequest);

            Order order = orderUpdateRequest.ToOrder();
            Order? updatedOrder = await _ordersRepository.UpdateOrder(order);
            
            if (updatedOrder is null)
            {
                throw new ArgumentException(nameof(updatedOrder));
            }

            return updatedOrder.ToOrderResponse();
        }
    }
}

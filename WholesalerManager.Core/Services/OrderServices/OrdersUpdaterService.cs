using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersUpdaterService : IOrdersUpdaterService
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrdersUpdaterService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
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

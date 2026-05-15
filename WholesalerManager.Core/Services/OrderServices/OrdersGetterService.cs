using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersGetterService : IOrdersGetterService
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrdersGetterService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public async Task<List<OrderResponse>> GetAllOrders()
        {
            var orders = await _ordersRepository.GetAllOrders();
            return orders.Select(o => o.ToOrderResponse()).ToList();
        }

        public async Task<OrderResponse?> GetOrderByID(Guid? orderID)
        {
            if (orderID is null)
            {
                return null;
            }
            var order = await _ordersRepository.GetOrderByID(orderID.Value);
            return order?.ToOrderResponse();
        }
    }
}

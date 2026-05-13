using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.DeliveryServices
{
    public class OrdersDeleterService : IOrdersDeleterService
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrdersDeleterService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public async Task<bool> DeleteOrderByID(Guid? orderID)
        {
            if (orderID is null)
            {
                throw new ArgumentNullException(nameof(orderID));
            }

            if (orderID == Guid.Empty)
            {
                throw new ArgumentException(nameof(orderID));
            }

            return await _ordersRepository.DeleteOrderById(orderID.Value);
        }
    }
}

using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;

namespace WholesalerManager.Core.Services.OrderServices
{
    public class OrdersAdderService : IOrdersAdderService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ICustomersGetterService _customersGetterService;

        public OrdersAdderService(IOrdersRepository ordersRepository, ICustomersGetterService customersGetterService)
        {
            _ordersRepository = ordersRepository;
            _customersGetterService = customersGetterService;
        }

        public async Task<OrderResponse> AddOrder(OrderAddRequest? orderAddRequest)
        {
            if (orderAddRequest is null)
            {
                throw new ArgumentNullException(nameof(orderAddRequest));
            }

            ValidationHelper.ModelValidation(orderAddRequest);

            var customer = await _customersGetterService.GetCustomerByID(orderAddRequest.CustomerID);
            if (customer is null)
            {
                throw new ArgumentException(nameof(customer));
            }

            var order = orderAddRequest.ToOrder();
            order.OrderID = Guid.NewGuid();
            var addedOrder = await _ordersRepository.AddOrder(order);

            return addedOrder.ToOrderResponse();
        }
    }
}

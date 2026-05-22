using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsGetterService : IOrderItemsGetterService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;
        private readonly ILogger<OrderItemsGetterService> _logger;

        public OrderItemsGetterService(IOrderItemsRepository orderItemsRepository, ILogger<OrderItemsGetterService> logger)
        {
            _orderItemsRepository = orderItemsRepository;
            _logger = logger;
        }


        public async Task<List<OrderItemResponse>> GetAllOrderItems()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllOrderItems), nameof(OrderItemsGetterService));
            var items = await _orderItemsRepository.GetAllOrderItems();
            return items.Select(i => i.ToOrderItemResponse()).ToList();
        }

        public async Task<List<OrderItemResponse>?> GetAllOrderItemsFromOrder(Guid? orderID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllOrderItemsFromOrder), nameof(OrderItemsGetterService));
            if (orderID is null)
            {
                _logger.LogError("{requestName} from {methodName} from {serviceName} is null.", nameof(orderID), nameof(GetAllOrderItemsFromOrder), nameof(OrderItemsGetterService));
                throw new ArgumentNullException(nameof(orderID));
            }
            var foundItems = await _orderItemsRepository.GetAllOrderItemsFromOrder(orderID.Value);
            return foundItems.Select(i => i.ToOrderItemResponse()).ToList();
        }
    }
}

using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;

namespace WholesalerManager.Core.Services.OrderItemServices
{
    public class OrderItemsGetterService : IOrderItemsGetterService
    {
        private readonly IOrderItemsRepository _orderItemsRepository;

        public OrderItemsGetterService(IOrderItemsRepository orderItemsRepository)
        {
            _orderItemsRepository = orderItemsRepository;
        }


        public async Task<List<OrderItemResponse>> GetAllOrderItems()
        {
            var items = await _orderItemsRepository.GetAllOrderItems();
            return items.Select(i => i.ToOrderItemResponse()).ToList();
        }

        public async Task<List<OrderItemResponse>?> GetAllOrderItemsFromOrder(Guid? orderID)
        {
            if (orderID is null)
            {
                return null;
            }
            var foundItems = await _orderItemsRepository.GetAllOrderItemsFromOrder(orderID.Value);
            return foundItems.Select(i => i.ToOrderItemResponse()).ToList();
        }
    }
}

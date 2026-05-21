using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    public interface IOrderUpdateCoordinatorService
    {
        Task<OrderResponse> UpdateFullOrder(OrderUpdateRequest? orderRequest, List<OrderItemUpdateRequest>? items);
    }
}

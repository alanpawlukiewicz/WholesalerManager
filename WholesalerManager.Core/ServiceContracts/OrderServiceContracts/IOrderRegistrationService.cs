using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    public interface IOrderRegistrationService
    {
        Task<OrderResponse> RegisterFullOrder(OrderAddRequest? orderRequest, List<OrderItemAddRequest>? items);
    }
}

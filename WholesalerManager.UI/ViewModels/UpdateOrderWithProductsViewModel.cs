using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class UpdateOrderWithProductsViewModel
    {
        public OrderUpdateRequest? Order { get; set; }
        public List<OrderItemUpdateRequest?>? Items { get; set; }
    }
}

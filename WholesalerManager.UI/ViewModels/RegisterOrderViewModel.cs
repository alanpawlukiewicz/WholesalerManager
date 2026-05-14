using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class RegisterOrderViewModel
    {
        public OrderAddRequest? OrderAddRequest { get; set; }

        public List<OrderItemAddRequest>? Items { get; set; }
    }
}

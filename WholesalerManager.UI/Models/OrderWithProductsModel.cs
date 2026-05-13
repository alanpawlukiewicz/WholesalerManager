using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;
using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.UI.Models
{
    public class OrderWithProductsModel
    {
        public OrderResponse? Order { get; set; }
        public List<OrderItemResponse>? Items { get; set; }
    }
}

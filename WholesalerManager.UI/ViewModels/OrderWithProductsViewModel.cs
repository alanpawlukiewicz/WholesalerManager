using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class OrderWithProductsViewModel
    {
        public OrderResponse? Order { get; set; }
        public List<OrderItemResponse>? Items { get; set; }
    }
}

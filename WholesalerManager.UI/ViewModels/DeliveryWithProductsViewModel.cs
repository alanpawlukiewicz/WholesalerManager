using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class DeliveryWithProductsViewModel
    {
        public DeliveryResponse? Delivery { get; set; }
        public List<DeliveryItemResponse>? Items { get; set; }
    }
}

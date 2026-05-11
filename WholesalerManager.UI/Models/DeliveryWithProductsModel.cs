using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.Models
{
    public class DeliveryWithProductsModel
    {
        public DeliveryResponse? Delivery { get; set; }
        public List<DeliveryItemResponse>? Items { get; set; }
    }
}

using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.Models
{
    public class UpdateDeliveryWithProductsModel
    {
        public DeliveryUpdateRequest? Delivery { get; set; }
        public List<DeliveryItemUpdateRequest?>? Items { get; set; }
    }
}

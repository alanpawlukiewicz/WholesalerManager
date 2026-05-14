using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.Models
{
    public class UpdateDeliveryWithProductsViewModel
    {
        public DeliveryUpdateRequest? Delivery { get; set; }
        public List<DeliveryItemUpdateRequest?>? Items { get; set; }
    }
}

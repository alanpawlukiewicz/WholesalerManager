using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class UpdateDeliveryWithProductsViewModel
    {
        public DeliveryUpdateRequest? Delivery { get; set; }
        public List<DeliveryItemUpdateRequest?>? Items { get; set; }
    }
}

using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.ViewModels
{
    public class RegisterDeliveryViewModel
    {
        public DeliveryAddRequest? DeliveryAddRequest { get; set; }

        public List<DeliveryItemAddRequest>? Items { get; set; }
    }
}

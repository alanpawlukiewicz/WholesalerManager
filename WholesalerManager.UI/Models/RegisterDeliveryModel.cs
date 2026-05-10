using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.UI.Models
{
    public class RegisterDeliveryModel
    {
        public DeliveryAddRequest? DeliveryAddRequest { get; set; }

        public List<DeliveryItemAddRequest>? Items { get; set; }
    }
}

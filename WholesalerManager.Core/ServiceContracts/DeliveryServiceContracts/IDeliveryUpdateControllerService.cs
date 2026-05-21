using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    public interface IDeliveryUpdateControllerService
    {
        Task<DeliveryResponse> UpdateFullDelivery(DeliveryUpdateRequest? deliveryUpdateRequest, List<DeliveryItemUpdateRequest>? items);
    }
}

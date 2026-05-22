using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    public interface IDeliveryUpdateCoordinatorService
    {
        Task<DeliveryResponse> UpdateFullDelivery(DeliveryUpdateRequest? deliveryUpdateRequest, List<DeliveryItemUpdateRequest>? items);
    }
}

using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Represents a service contract for registering a full delivery, including its details and associated items.
    /// </summary>
    public interface IDeliveryRegistrationService
    {
        /// <summary>
        /// Asynchronously registers a full delivery, including its details and associated items, based on the provided delivery add request and list of delivery item add requests.
        /// </summary>
        /// <param name="deliveryAddRequest">The request containing the delivery information to be registered.</param>
        /// <param name="items">The list of requests containing the delivery item information to be registered.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the registered delivery response.</returns>
        Task<DeliveryResponse> RegisterFullDelivery(DeliveryAddRequest? deliveryAddRequest, List<DeliveryItemAddRequest>? items);
    }
}

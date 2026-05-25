using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Represents a service contract for coordinating the update of a delivery, including its associated delivery items. This service is responsible for handling the logic required to update both the delivery details and the related delivery items in a cohesive manner.
    /// </summary>
    public interface IDeliveryUpdateCoordinatorService
    {
        /// <summary>
        /// Asynchronously updates a delivery along with its associated delivery items. This method takes in a DeliveryUpdateRequest object containing the updated delivery information and a list of DeliveryItemUpdateRequest objects representing the updated delivery items. The service will handle the necessary operations to ensure that both the delivery and its items are updated correctly, maintaining data integrity and consistency throughout the process.
        /// </summary>
        /// <param name="deliveryUpdateRequest">The request containing the updated delivery information.</param>
        /// <param name="items">The list of requests containing the updated delivery item information.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the updated delivery response.</returns>
        Task<DeliveryResponse> UpdateFullDelivery(DeliveryUpdateRequest? deliveryUpdateRequest, List<DeliveryItemUpdateRequest>? items);
    }
}

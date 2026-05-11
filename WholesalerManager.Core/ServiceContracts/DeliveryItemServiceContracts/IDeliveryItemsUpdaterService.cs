using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts
{
    /// <summary>
    /// Represents business logic for updating DeliveryItem table records.
    /// </summary>
    public interface IDeliveryItemsUpdaterService
    {
        /// <summary>
        /// Updates an existing delivery item with the specified changes.
        /// </summary>
        /// <param name="deliveryItemUpdateRequest">The request containing the updated values for the delivery item. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with the updated
        /// delivery item details, or null if the item was not found.</returns>
        Task<DeliveryItemResponse> UpdateDeliveryItem(DeliveryItemUpdateRequest? deliveryItemUpdateRequest);

        /// <summary>
        /// Updates multiple delivery items based on the provided update requests.
        /// </summary>
        /// <param name="deliveryItemUpdateRequests">A list of delivery item update requests specifying the changes to apply to each delivery item. Cannot be
        /// null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of responses for each
        /// updated delivery item. Each element corresponds to the result of updating the respective item in the input
        /// list; elements may be null if an update could not be performed.</returns>
        Task<List<DeliveryItemResponse>> UpdateMultipleDeliveryItems(List<DeliveryItemUpdateRequest?>? deliveryItemUpdateRequests);
    }
}

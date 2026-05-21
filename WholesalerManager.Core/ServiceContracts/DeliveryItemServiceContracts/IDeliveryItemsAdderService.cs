using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts
{
    /// <summary>
    /// Represents business logic responsible for adding multiple Products to multiple deliveries
    /// </summary>
    public interface IDeliveryItemsAdderService
    {
        /// <summary>
        /// Asynchronously adds new delivery item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<DeliveryItemResponse> AddDeliveryItem(DeliveryItemAddRequest? itemAddRequest);

        /// <summary>
        /// Asynchronously adds multiple delivery items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<List<DeliveryItemResponse>> AddMultipleDeliveryItems(List<DeliveryItemAddRequest>? itemAddRequests);
    }
}

using WholesalerManager.Core.DTO.DeliveryItemDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts
{
    /// <summary>
    /// Represents buisness layer logic for connection between Delivery and Product services.
    /// </summary>
    public interface IDeliveryItemsGetterService
    {
        /// <summary>
        /// Asynchronously retrieves all delivery items.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DeliveryItemResponse"/> objects representing all delivery items. The list is empty if no delivery
        /// items are found.</returns>
        Task<List<DeliveryItemResponse>> GetAllDeliveryItems();

        /// <summary>
        /// Asynchronously retrieves all delivery items connected to chosen delivery.
        /// </summary>
        /// <param name="deliveryID"></param>
        /// <returns></returns>
        Task<List<DeliveryItemResponse>?> GetAllDeliveryItemsFromDelivery(Guid? deliveryID);
    }
}

using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents data layer logic of n:n connection between Delivery and Product tables.
    /// </summary>
    public interface IDeliveryItemsRepository
    {
        /// <summary>
        /// Asynchronously retrieves all products connected to all deliveries.
        /// </summary>
        /// <returns></returns>
        Task<List<DeliveryItem>> GetAllDeliveryItems();

        /// <summary>
        /// Asynchronously retrieves all products associated with the specified delivery.
        /// </summary>
        /// <param name="deliveryID">The unique identifier of the delivery for which to retrieve products.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products included in
        /// the specified delivery. The list is empty if the delivery contains no products.</returns>
        Task<List<DeliveryItem>> GetAllDeliveryItemsFromDelivery(Guid deliveryID);

        /// <summary>
        /// Asynchronously adds a new delivery item to the system.
        /// </summary>
        /// <param name="item">The delivery item to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added delivery item,
        /// including any updated properties set by the system.</returns>
        Task<DeliveryItem> AddDeliveryItem(DeliveryItem item);

        /// <summary>
        /// Asynchronously adds all items from given list.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<List<DeliveryItem>> AddMultipleDeliveryItems(List<DeliveryItem> items);

        /// <summary>
        /// Asynchronously updates delivery item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<DeliveryItem?> UpdateDeliveryItem(DeliveryItem item);

        /// <summary>
        /// Updates multiple delivery items in a single operation.
        /// </summary>
        /// <remarks>The method performs updates in bulk, which may improve performance compared to
        /// updating items individually. The returned list contains the updated delivery items in the same order as the
        /// input list.</remarks>
        /// <param name="items">A list of <see cref="DeliveryItem"/> objects to update. Cannot be null or empty. Each item in the list
        /// represents a delivery item to be updated.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DeliveryItem"/> objects reflecting the updated state after the operation completes.</returns>
        Task<List<DeliveryItem?>> UpdateMultipleDeliveryItems(List<DeliveryItem> items);

        /// <summary>
        /// Deletes the specified delivery item asynchronously.
        /// </summary>
        /// <param name="item">The delivery item to delete. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result contains the deleted delivery
        /// item.</returns>
        Task<bool> DeleteDeliveryItem(DeliveryItem item);
    }
}

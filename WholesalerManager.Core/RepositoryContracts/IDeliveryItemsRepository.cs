using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
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
    }
}

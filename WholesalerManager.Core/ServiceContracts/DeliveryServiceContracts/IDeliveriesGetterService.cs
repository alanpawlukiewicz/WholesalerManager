using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Represents business logic for retrieving delivery data.
    /// </summary>
    public interface IDeliveriesGetterService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all deliveries.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DeliveryResponse"/> objects representing all deliveries. The list will be empty if no deliveries are
        /// found.</returns>
        Task<List<DeliveryResponse>> GetAllDeliveries();

        /// <summary>
        /// Asynchronously retrieves a delivery by its unique identifier (DeliveryID).
        /// </summary>
        /// <param name="deliveryID">The unique identifier of the delivery to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DeliveryResponse"/> object representing the delivery if found; otherwise, null.</returns>
        Task<DeliveryResponse?> GetDeliveryById(Guid? deliveryID);

        /// <summary>
        /// Asynchronously retrieves a list of deliveries filtered by a specified property and filter value, with an option to ignore case sensitivity.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter by.</param>
        /// <param name="filter">The filter value.</param>
        /// <param name="ignoreCase">Indicates whether to ignore case sensitivity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="DeliveryResponse"/> objects representing the filtered deliveries.</returns>
        Task<List<DeliveryResponse>> GetFilteredDeliveries(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of deliveries sorted based on a specified property and sort order.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="sortOrder">The sort order (ascending or descending).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="DeliveryResponse"/> objects representing the sorted deliveries.</returns>
        Task<List<DeliveryResponse>> GetSortedDeliveries(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

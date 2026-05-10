using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;

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
    }
}

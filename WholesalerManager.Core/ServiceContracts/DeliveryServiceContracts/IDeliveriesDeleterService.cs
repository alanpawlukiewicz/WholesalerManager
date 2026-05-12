using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Defines a service for deleting delivery records by their unique identifier.
    /// </summary>
    public interface IDeliveriesDeleterService
    {
        /// <summary>
        /// Deletes the delivery record with the specified unique identifier. Function also deletes connected delivery items.
        /// </summary>
        /// <param name="deliveryID">The unique identifier of the delivery to delete. If null, the method will not perform any deletion.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the delivery
        /// was successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<bool> DeleteDeliveryByID(Guid? deliveryID);
    }
}

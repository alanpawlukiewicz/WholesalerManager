using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Represents business logic for updating Delivery table records.
    /// </summary>
    public interface IDeliveriesUpdaterService
    {
        /// <summary>
        /// Asynchronously updates delivery record.
        /// </summary>
        /// <param name="deliveryUpdateRequest"></param>
        /// <returns></returns>
        Task<DeliveryResponse> UpdateDelivery(DeliveryUpdateRequest? deliveryUpdateRequest);
    }
}

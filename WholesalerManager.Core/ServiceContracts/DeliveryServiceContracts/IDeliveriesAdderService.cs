using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;

namespace WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts
{
    /// <summary>
    /// Represents buisness logic for registering new deliveries to database.
    /// </summary>
    public interface IDeliveriesAdderService
    {
        /// <summary>
        /// Asynchronously adds new delivery to database
        /// </summary>
        /// <param name="deliveryAddRequest"></param>
        /// <returns></returns>
        Task<DeliveryResponse> AddDelivery(DeliveryAddRequest? deliveryAddRequest);
    }
}

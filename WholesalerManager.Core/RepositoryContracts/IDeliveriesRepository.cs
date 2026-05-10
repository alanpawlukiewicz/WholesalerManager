using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.RepositoryContracts
{
    /// <summary>
    /// Represents data logic for managing deliveries.
    /// </summary>
    public interface IDeliveriesRepository
    {
        /// <summary>
        /// Asynchronously retrieves all deliveries from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of deliveries.</returns>
        Task<List<Delivery>> GetAllDeliveries();

        /// <summary>
        /// Asynchronously retrieves a delivery by its unique identifier.
        /// </summary>
        /// <param name="deliveryID">The unique identifier of the delivery to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the delivery if found; otherwise, null.</returns>
        Task<Delivery?> GetDeliveryById(Guid deliveryID);

        /// <summary>
        /// Asynchronously adds new delivery to database.
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        Task<Delivery> AddDelivery(Delivery delivery);
    }
}

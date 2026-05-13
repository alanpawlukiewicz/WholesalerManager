using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.DTO.OrderDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Represents business logic for retrieving order data.
    /// </summary>
    public interface IOrdersGetterService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all orders.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="OrderResponse"/> objects representing all orders. The list will be empty if no deliveries are
        /// found.</returns>
        Task<List<OrderResponse>> GetAllOrders();

        /// <summary>
        /// Asynchronously retrieves a order by its unique identifier (OrderID).
        /// </summary>
        /// <param name="orderID">The unique identifier of the order to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="OrderResponse"/> object representing the order if found; otherwise, null.</returns>
        Task<OrderResponse?> GetOrderByID(Guid? orderID);
    }
}

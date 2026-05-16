using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.OrderDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Represents business logic for updating Order table records.
    /// </summary>
    public interface IOrdersUpdaterService
    {
        /// <summary>
        /// Asynchronously updates order record.
        /// </summary>
        /// <param name="orderUpdateRequest"></param>
        /// <returns></returns>
        Task<OrderResponse> UpdateOrder(OrderUpdateRequest? orderUpdateRequest);

        /// <summary>
        /// Attempts to cancel the order with the specified identifier asynchronously.
        /// </summary>
        /// <param name="orderID">The unique identifier of the order to cancel.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the order
        /// was successfully canceled; otherwise, <see langword="false"/>.</returns>
        Task<bool> CancelOrder(Guid orderID);
    }
}

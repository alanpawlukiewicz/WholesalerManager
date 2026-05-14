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
    }
}

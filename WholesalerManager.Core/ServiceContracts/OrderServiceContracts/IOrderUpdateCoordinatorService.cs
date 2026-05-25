using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Represents a service contract for coordinating the update of an order, including both the order details and its associated items. This service is responsible for handling the logic required to update an existing order in the system, ensuring that all related data is correctly modified and consistent with the business rules.
    /// </summary>
    public interface IOrderUpdateCoordinatorService
    {
        /// <summary>
        /// Asynchronously updates an existing order along with its associated items based on the provided order update request and list of item update requests. This method ensures that the order and its items are updated in a coordinated manner, maintaining data integrity and adhering to any business rules defined for order updates.
        /// </summary>
        /// <param name="orderRequest">The request containing the order information to be updated.</param>
        /// <param name="items">The list of requests containing the order item information to be updated.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the updated order response.</returns>
        Task<OrderResponse> UpdateFullOrder(OrderUpdateRequest? orderRequest, List<OrderItemUpdateRequest>? items);
    }
}

using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts
{
    /// <summary>
    /// Represents business logic for updating OrderItem table records.
    /// </summary>
    public interface IOrderItemsUpdaterService
    {
        /// <summary>
        /// Updates an existing order item with the specified changes.
        /// </summary>
        /// <param name="orderItemUpdateRequest">The request containing the updated values for the order item. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with the updated
        /// order item details, or null if the item was not found.</returns>
        Task<OrderItemResponse> UpdateOrderItem(OrderItemUpdateRequest? orderItemUpdateRequest);

        /// <summary>
        /// Updates multiple order items based on the provided update requests.
        /// </summary>
        /// <param name="orderItemUpdateRequests">A list of order item update requests specifying the changes to apply to each order item. Cannot be
        /// null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of responses for each
        /// updated order item. Each element corresponds to the result of updating the respective item in the input
        /// list; elements may be null if an update could not be performed.</returns>
        Task<List<OrderItemResponse>> UpdateMultipleOrderItems(List<OrderItemUpdateRequest?>? orderItemUpdateRequests);
    }
}

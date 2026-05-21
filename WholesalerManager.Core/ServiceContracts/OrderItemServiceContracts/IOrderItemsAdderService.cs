using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts
{
    /// <summary>
    /// Represents business logic responsible for adding multiple Products to multiple deliveries
    /// </summary>
    public interface IOrderItemsAdderService
    {
        /// <summary>
        /// Asynchronously adds new order item.
        /// </summary>
        /// <param name="itemAddRequest"></param>
        /// <returns></returns>
        Task<OrderItemResponse> AddOrderItem(OrderItemAddRequest? itemAddRequest);

        /// <summary>
        /// Asynchronously adds multiple order items.
        /// </summary>
        /// <param name="itemAddRequests"></param>
        /// <returns></returns>
        Task<List<OrderItemResponse>> AddMultipleOrderItems(List<OrderItemAddRequest>? itemAddRequests);
    }
}

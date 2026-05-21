using WholesalerManager.Core.DTO.OrderItemDTO;

namespace WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts
{
    /// <summary>
    /// Represents buisness layer logic for connection between Order and Product services.
    /// </summary>
    public interface IOrderItemsGetterService
    {
        /// <summary>
        /// Asynchronously retrieves all order items.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="OrderItemResponse"/> objects representing all order items. The list is empty if no order
        /// items are found.</returns>
        Task<List<OrderItemResponse>> GetAllOrderItems();

        /// <summary>
        /// Asynchronously retrieves all order items connected to chosen order.
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<List<OrderItemResponse>?> GetAllOrderItemsFromOrder(Guid? orderID);
    }
}

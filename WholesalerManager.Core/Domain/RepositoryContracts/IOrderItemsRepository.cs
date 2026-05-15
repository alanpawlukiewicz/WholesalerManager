using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents data layer logic of n:n connection between Order and Product tables.
    /// </summary>
    public interface IOrderItemsRepository
    {
        /// <summary>
        /// Asynchronously retrieves all products connected to all deliveries.
        /// </summary>
        /// <returns></returns>
        Task<List<OrderItem>> GetAllOrderItems();

        /// <summary>
        /// Asynchronously retrieves all products associated with the specified order.
        /// </summary>
        /// <param name="orderID">The unique identifier of the order for which to retrieve products.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products included in
        /// the specified order. The list is empty if the order contains no products.</returns>
        Task<List<OrderItem>> GetAllOrderItemsFromOrder(Guid orderID);

        /// <summary>
        /// Asynchronously adds a new order item to the system.
        /// </summary>
        /// <param name="item">The order item to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added order item,
        /// including any updated properties set by the system.</returns>
        Task<OrderItem> AddOrderItem(OrderItem item);

        /// <summary>
        /// Asynchronously adds all items from given list.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<List<OrderItem>> AddMultipleOrderItems(List<OrderItem> items);

        /// <summary>
        /// Asynchronously updates order item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<OrderItem?> UpdateOrderItem(OrderItem item);

        /// <summary>
        /// Updates multiple order items in a single operation.
        /// </summary>
        /// <remarks>The method performs updates in bulk, which may improve performance compared to
        /// updating items individually. The returned list contains the updated order items in the same order as the
        /// input list.</remarks>
        /// <param name="items">A list of <see cref="OrderItem"/> objects to update. Cannot be null or empty. Each item in the list
        /// represents a order item to be updated.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="OrderItem"/> objects reflecting the updated state after the operation completes.</returns>
        Task<List<OrderItem?>> UpdateMultipleOrderItems(List<OrderItem> items);

        /// <summary>
        /// Deletes the specified order item asynchronously.
        /// </summary>
        /// <param name="item">The order item to delete. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result contains the deleted order
        /// item.</returns>
        Task<bool> DeleteOrderItem(OrderItem item);
    }
}

using WholesalerManager.Core.DTO.OrderDTO;
using WholesalerManager.Core.Enums;

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

        /// <summary>
        /// Asynchronously retrieves a list of orders filtered by a specified property and filter value. The filtering can be case-insensitive based on the ignoreCase parameter.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="filter"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        Task<List<OrderResponse>> GetFilteredOrders(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of orders sorted by a specified property and sort order (ascending or descending).
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<List<OrderResponse>> GetSortedOrders(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

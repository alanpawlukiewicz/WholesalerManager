using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents data layer logic for manipulation Order table data.
    /// </summary>
    public interface IOrdersRepository
    {
        /// <summary>
        /// Asynchronously retrieves all orders for database.
        /// </summary>
        /// <returns></returns>
        Task<List<Order>> GetAllOrders();

        /// <summary>
        /// Asynchronously retrieves order data with given id.
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task<Order?> GetOrderByID(Guid orderID);

        /// <summary>
        /// Asynchronously adds new order to database.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<Order> AddOrder(Order order);

        /// <summary>
        /// Asynchronously updates order data.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<Order?> UpdateOrder(Order order);

        /// <summary>
        /// Asynchronously saves data to database.
        /// </summary>
        /// <returns></returns>
        Task<int> Save();

        /// <summary>
        /// Asynchronously deletes order data and associated OrderItem records.
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>True if deleted, false if not.</returns>
        Task<bool> DeleteOrderById(Guid OrderID);
    }
}

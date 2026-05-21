namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Defines a service for asynchronously checking whether sufficient stock is available to fulfill a specified order.
    /// </summary>
    public interface IOrdersStockCheckerService
    {
        /// <summary>
        /// Asynchronously determines whether sufficient stock is available to fulfill the specified order.
        /// </summary>
        /// <param name="orderID">The unique identifier of the order to check for stock availability. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if
        /// sufficient stock is available for the order; otherwise, <see langword="false"/>.</returns>
        Task<bool> CheckStockAvailabilityForOrder(Guid? orderID);
    }
}

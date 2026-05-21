namespace WholesalerManager.Core.ServiceContracts.OrderServiceContracts
{
    /// <summary>
    /// Defines a service for deleting order records by their unique identifier.
    /// </summary>
    public interface IOrdersDeleterService
    {
        /// <summary>
        /// Deletes the order record with the specified unique identifier. Function also deletes connected order items.
        /// </summary>
        /// <param name="orderID">The unique identifier of the order to delete. If null, the method will not perform any deletion.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the order
        /// was successfully deleted; otherwise, <see langword="false"/>.</returns>
        Task<bool> DeleteOrderByID(Guid? orderID);
    }
}

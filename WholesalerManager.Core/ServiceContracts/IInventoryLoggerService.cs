using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts
{
    /// <summary>
    /// Represents a service contract for managing inventory logs. This interface defines methods for retrieving, filtering, and sorting inventory logs.
    /// </summary>
    public interface IInventoryLoggerService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all inventory logs. This method is intended to provide a comprehensive view of all inventory changes, including details such as the type of operation, quantity changed, and timestamps.
        /// </summary>
        /// <returns>List of inventory logs</returns>
        Task<List<InventoryLog>> GetInventoryLogsAsync();

        /// <summary>
        /// Asynchronously retrieves a list of inventory logs filtered by a specified property and filter value.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter by</param>
        /// <param name="filter">The filter value</param>
        /// <param name="ignoreCase">Indicates whether to perform case-insensitive filtering</param>
        /// <returns>List of filtered inventory logs</returns>
        Task<List<InventoryLog>> GetFilteredInventoryLogs(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of inventory logs sorted by a specified property and sort order.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by</param>
        /// <param name="sortOrder">The sort order</param>
        /// <returns>List of sorted inventory logs</returns>
        Task<List<InventoryLog>> GetSortedInventoryLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

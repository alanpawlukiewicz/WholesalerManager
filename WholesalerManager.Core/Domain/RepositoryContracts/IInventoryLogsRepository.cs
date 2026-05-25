using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents a repository for managing inventory logs in the application. This interface defines methods for retrieving inventory log data, allowing for abstraction and separation of concerns in the data access layer.
    /// </summary>
    public interface IInventoryLogsRepository
    {
        /// <summary>
        /// Retrieves all inventory logs from the data source. This method is asynchronous and returns a list of InventoryLog entities, which contain details about inventory changes, including the type of operation, quantity changed, previous and new stock levels, and associated product, order, or delivery information.
        /// </summary>
        /// <returns>List of inventory logs.</returns>
        Task<List<InventoryLog>> GetAllInventoryLogsAsync();
    }
}

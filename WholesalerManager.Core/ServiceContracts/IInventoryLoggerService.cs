using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts
{
    public interface IInventoryLoggerService
    {
        Task<List<InventoryLog>> GetInventoryLogsAsync();

        Task<List<InventoryLog>> GetFilteredInventoryLogs(string? propertyName, string? filter, bool ignoreCase = true);

        Task<List<InventoryLog>> GetSortedInventoryLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

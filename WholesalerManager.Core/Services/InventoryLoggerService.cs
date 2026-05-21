using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.DeliveryDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class InventoryLoggerService : IInventoryLoggerService
    {
        private readonly IInventoryLogsRepository _inventoryLogsRepository;
        private readonly ILogger<InventoryLoggerService> _logger;

        public InventoryLoggerService(IInventoryLogsRepository inventoryLogsRepository, ILogger<InventoryLoggerService> logger)
        {
            _inventoryLogsRepository = inventoryLogsRepository;
            _logger = logger;
        }

        public async Task<List<InventoryLog>> GetFilteredInventoryLogs(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredInventoryLogs), nameof(InventoryLoggerService));

            var allLogs = await _inventoryLogsRepository.GetAllInventoryLogsAsync();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                _logger.LogInformation("{methodName} from {serviceName} returning all products.", nameof(GetFilteredInventoryLogs), nameof(InventoryLoggerService));
                return allLogs;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(InventoryLog.CreatedAt):
                    if (DateTime.TryParse(filter, out var filterDate))
                    {
                        var startDate = filterDate.Date;
                        var endDate = startDate.AddDays(1);

                        return allLogs.Where(l => l.CreatedAt >= startDate && l.CreatedAt < endDate).ToList();
                    }
                    return new List<InventoryLog>();
                case nameof(InventoryLog.OperationType):
                    return allLogs.Where(l => l.OperationType != null
                    && l.OperationType.Contains(filter, stringComparisonType)).ToList();
                case nameof(InventoryLog.Product.ProductName):
                    return allLogs.Where(l => l.Product != null 
                    && l.Product.ProductName != null
                    && l.Product.ProductName.Contains(filter, stringComparisonType)).ToList();
                case nameof(InventoryLog.PreviousStock):
                    return allLogs.Where(l =>  l.PreviousStock.ToString().Contains(filter, stringComparisonType)).ToList();
                case nameof(InventoryLog.NewStock):
                    return allLogs.Where(l => l.NewStock.ToString().Contains(filter, stringComparisonType)).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<List<InventoryLog>> GetInventoryLogsAsync()
        {
            return await _inventoryLogsRepository.GetAllInventoryLogsAsync();
        }

        public async Task<List<InventoryLog>> GetSortedInventoryLogs(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedInventoryLogs), nameof(InventoryLoggerService));

            var allLogs = await _inventoryLogsRepository.GetAllInventoryLogsAsync();

            if (string.IsNullOrEmpty(propertyName))
            {
                return allLogs;
            }

            switch (propertyName)
            {
                case nameof(InventoryLog.CreatedAt):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.CreatedAt).ToList()
                        : allLogs.OrderByDescending(d => d.CreatedAt).ToList();
                case nameof(InventoryLog.OperationType):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.OperationType).ToList()
                        : allLogs.OrderByDescending(d => d.OperationType).ToList();
                case nameof(InventoryLog.Product.ProductName):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.Product?.ProductName).ToList()
                        : allLogs.OrderByDescending(d => d.Product?.ProductName).ToList();
                case nameof(InventoryLog.PreviousStock):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.PreviousStock).ToList()
                        : allLogs.OrderByDescending(d => d.PreviousStock).ToList();
                case nameof(InventoryLog.NewStock):
                    return sortOrder == SortOrderOptions.ASC
                        ? allLogs.OrderBy(d => d.NewStock).ToList()
                        : allLogs.OrderByDescending(d => d.NewStock).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }
    }
}

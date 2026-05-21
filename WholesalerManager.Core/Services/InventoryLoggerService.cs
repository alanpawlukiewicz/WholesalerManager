using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class InventoryLoggerService : IInventoryLoggerService
    {
        private readonly IInventoryLogsRepository _inventoryLogsRepository;

        public InventoryLoggerService(IInventoryLogsRepository inventoryLogsRepository)
        {
            _inventoryLogsRepository = inventoryLogsRepository;
        }

        public async Task<List<InventoryLog>> GetInventoryLogsAsync()
        {
            return await _inventoryLogsRepository.GetAllInventoryLogsAsync();
        }
    }
}

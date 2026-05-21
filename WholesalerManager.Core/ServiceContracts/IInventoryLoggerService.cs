using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.ServiceContracts
{
    public interface IInventoryLoggerService
    {
        Task<List<InventoryLog>> GetInventoryLogsAsync();
    }
}

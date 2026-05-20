using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    public interface IAuditLogsRepository
    {
        Task<bool> AddAuditLogAsync(AuditLog auditLog);

        Task<List<AuditLog>> GetAllAuditLogsAsync();
    }
}

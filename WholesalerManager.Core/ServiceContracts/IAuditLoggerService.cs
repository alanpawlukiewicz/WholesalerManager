using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.ServiceContracts
{
    public interface IAuditLoggerService
    {
        Task<bool> LogLoginAttempt(Guid? userID, string attemptedUsername, bool success);
    }
}

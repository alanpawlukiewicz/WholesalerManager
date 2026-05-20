using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WholesalerManager.Core.Domain.Entities
{
    public class AuditLog
    {
        public Guid AuditLogID { get; set; }

        public Guid? UserID { get; set; }

        public string AttemptedUsername { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(40)]
        public string? IpAddress { get; set; }

        public bool Success { get; set; } = false;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.Core.Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid AuditLogID { get; set; }

        public Guid? UserID { get; set; }

        [StringLength(256)]
        public string AttemptedUsername { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(40)]
        public string? IpAddress { get; set; }

        public bool Success { get; set; } = false;

        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; }
    }
}

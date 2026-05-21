using Microsoft.AspNetCore.Identity;

namespace WholesalerManager.Core.Domain.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool MustChangePassword { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
    }
}

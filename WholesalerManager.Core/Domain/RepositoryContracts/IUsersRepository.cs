using Microsoft.AspNetCore.Identity;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    public interface IUsersRepository
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();

        Task<ApplicationUser?> GetUserByIdAsync(Guid id);

        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        Task<IdentityResult> UpdateUser(ApplicationUser user);

        Task<IdentityResult> AddUser(ApplicationUser user, string password);

        Task<IdentityResult> DeleteUser(Guid id);

        Task<IdentityResult> AddNewRoleToUser(ApplicationUser user, string roleName);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    }
}

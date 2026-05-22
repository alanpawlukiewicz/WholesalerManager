using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.RepositoryContracts;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersRepository> _logger;

        public UsersRepository(UserManager<ApplicationUser> userManager, ILogger<UsersRepository> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IdentityResult> AddNewRoleToUser(ApplicationUser user, string roleName)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddNewRoleToUser), nameof(UsersRepository));

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> AddUser(ApplicationUser user, string password)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(AddUser), nameof(UsersRepository));

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> DeleteUser(Guid id)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteUser), nameof(UsersRepository));

            var matchingUser = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (matchingUser is null)
            {
                _logger.LogWarning("User not found.");
                return IdentityResult.Failed();
            }
            IdentityResult result = await _userManager.DeleteAsync(matchingUser);
            return result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GeneratePasswordResetTokenAsync), nameof(UsersRepository));

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllUsersAsync), nameof(UsersRepository));

            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetUserByEmailAsync), nameof(UsersRepository));

            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetUserByIdAsync), nameof(UsersRepository));

            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser?> GetUserByNameAsync(string name)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetUserByNameAsync), nameof(UsersRepository));

            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == name);
        }

        public async Task<IdentityResult> UpdateUser(ApplicationUser user)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateUser), nameof(UsersRepository));

            ApplicationUser? matchingUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (matchingUser is null)
            {
                _logger.LogWarning("User not found.");
                return IdentityResult.Failed();
            }

            matchingUser.FirstName = user.FirstName;
            matchingUser.LastName = user.LastName;
            matchingUser.UserName = user.UserName;
            matchingUser.Email = user.Email;
            matchingUser.PhoneNumber = user.PhoneNumber;

            IdentityResult result = await _userManager.UpdateAsync(matchingUser);
            return result;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersUpdaterService : IUsersUpdaterService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<UsersUpdaterService> _logger;

        public UsersUpdaterService(IUsersRepository usersRepository, ILogger<UsersUpdaterService> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<bool> ChangeEnabledStatus(Guid userID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(ChangeEnabledStatus), nameof(UsersUpdaterService));

            var matchingUser = await _usersRepository.GetUserByIdAsync(userID);
            if (matchingUser == null)
            {
                return false;
            }

            matchingUser.IsEnabled = !matchingUser.IsEnabled;
            await _usersRepository.UpdateUser(matchingUser);
            return true;
        }

        public async Task<IdentityResult> MakeUserChangePassword(Guid id)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(MakeUserChangePassword), nameof(UsersUpdaterService));

            var matchingUser = await _usersRepository.GetUserByIdAsync(id);
            if (matchingUser is null)
            {
                return IdentityResult.Failed();
            }
            matchingUser.MustChangePassword = true;

            return await _usersRepository.UpdateUser(matchingUser);
        }

        public async Task<IdentityResult> UpdateUserAsync(UserEditRequest userEditRequest)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(UpdateUserAsync), nameof(UsersUpdaterService));

            ValidationHelper.ModelValidation(userEditRequest);

            return await _usersRepository.UpdateUser(userEditRequest.ToApplicationUser());
        }
    }
}

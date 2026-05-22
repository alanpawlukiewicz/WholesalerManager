using Microsoft.AspNetCore.Identity;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersUpdaterService : IUsersUpdaterService
    {
        private readonly IUsersRepository _usersRepository;

        public UsersUpdaterService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> ChangeEnabledStatus(Guid userID)
        {
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
            ValidationHelper.ModelValidation(userEditRequest);

            return await _usersRepository.UpdateUser(userEditRequest.ToApplicationUser());
        }
    }
}

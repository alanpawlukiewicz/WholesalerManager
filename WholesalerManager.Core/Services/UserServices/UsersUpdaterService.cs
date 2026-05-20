using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
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
            var result = await _usersRepository.UpdateUser(matchingUser);
            return result.Succeeded;
        }

        public async Task<IdentityResult> UpdateUserAsync(UserEditRequest userEditRequest)
        {
            return await _usersRepository.UpdateUser(userEditRequest.ToApplicationUser());
        }
    }
}

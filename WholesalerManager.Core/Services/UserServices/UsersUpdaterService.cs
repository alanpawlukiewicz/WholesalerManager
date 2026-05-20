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

        public async Task<IdentityResult> UpdateUserAsync(UserEditRequest userEditRequest)
        {
            return await _usersRepository.UpdateUser(userEditRequest.ToApplicationUser());
        }
    }
}

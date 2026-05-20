using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersGetterService
    {
        Task<UserResponse?> GetUserByIdAsync(Guid userId);

        Task<List<UserResponse>> GetAllUsersAsync();

        Task<string> GeneratePasswordResetTokenAsync(Guid id);
    }
}

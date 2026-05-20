using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersUpdaterService
    {
        Task<IdentityResult> UpdateUserAsync(UserEditRequest userEditRequest);

        Task<IdentityResult> MakeUserChangePassword(Guid id);

        Task<bool> ChangeEnabledStatus(Guid userID);
    }
}

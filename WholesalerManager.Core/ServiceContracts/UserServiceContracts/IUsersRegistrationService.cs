using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersRegistrationService
    {
        Task<UserResponse?> RegisterUserAsync(RegisterDTO registerData);
    }
}

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersDeleterService
    {
        Task<bool> DeleteUser(Guid userID);
    }
}

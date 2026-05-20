using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersDeleterService : IUsersDeleterService
    {
        private readonly IUsersRepository _usersRepository;

        public UsersDeleterService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<bool> DeleteUser(Guid userID)
        {
            var result = await _usersRepository.DeleteUser(userID);
            return result.Succeeded;
        }
    }
}

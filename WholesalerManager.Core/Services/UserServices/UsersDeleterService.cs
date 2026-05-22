using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersDeleterService : IUsersDeleterService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<UsersDeleterService> _logger;

        public UsersDeleterService(IUsersRepository usersRepository, ILogger<UsersDeleterService> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteUser(Guid userID)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(DeleteUser), nameof(UsersDeleterService));

            var result = await _usersRepository.DeleteUser(userID);
            return result.Succeeded;
        }
    }
}

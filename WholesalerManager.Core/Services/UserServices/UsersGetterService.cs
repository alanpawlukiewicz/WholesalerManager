using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Enums;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersGetterService : IUsersGetterService
    {
        private readonly IUsersRepository _usersRepository;

        private readonly ILogger<UsersGetterService> _logger;

        public UsersGetterService(IUsersRepository usersRepository, ILogger<UsersGetterService> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Guid id)
        {
            _logger.LogInformation("Attempting to generate password reset token for user with ID {UserId}", id);

            ApplicationUser? user = await _usersRepository.GetUserByIdAsync(id);
            if (user is null)
            {
                _logger.LogWarning("No user found with ID {UserId} when attempting to generate password reset token.", id);
                throw new InvalidOperationException($"No user found with ID {id}");
            }
            return await _usersRepository.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetAllUsersAsync), nameof(UsersGetterService));

            var allUsers = await _usersRepository.GetAllUsersAsync();
            return allUsers.Select(usr => usr.ToUserResponse()).ToList();
        }

        public async Task<List<UserResponse>> GetFilteredUsers(string? propertyName, string? filter, bool ignoreCase = true)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetFilteredUsers), nameof(UsersGetterService));

            var allUsers = await _usersRepository.GetAllUsersAsync();
            var userResponses = allUsers.Select(u => u.ToUserResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(filter))
            {
                _logger.LogInformation("{methodName} from {serviceName} returning all products.", nameof(GetFilteredUsers), nameof(UsersGetterService));
                return userResponses;
            }

            StringComparison stringComparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (propertyName)
            {
                case nameof(UserResponse.FirstName):
                    return userResponses.Where(u => u.FirstName != null && u.FirstName.Contains(filter, stringComparisonType)).ToList();
                case nameof(UserResponse.LastName):
                    return userResponses.Where(u => u.LastName != null && u.LastName.Contains(filter, stringComparisonType)).ToList();
                case nameof(UserResponse.UserName):
                    return userResponses.Where(u => u.UserName != null && u.UserName.Contains(filter, stringComparisonType)).ToList();
                case nameof(UserResponse.Email):
                    return userResponses.Where(u => u.Email != null && u.Email.Contains(filter, stringComparisonType)).ToList();
                case nameof(UserResponse.PhoneNumber):
                    return userResponses.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(filter, stringComparisonType)).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }
        }

        public async Task<List<UserResponse>> GetSortedUsers(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetSortedUsers), nameof(UsersGetterService));

            var allUsers = await _usersRepository.GetAllUsersAsync();
            var userResponses = allUsers.Select(u => u.ToUserResponse()).ToList();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return userResponses;
            }

            switch (propertyName)
            {
                case nameof(UserResponse.FirstName):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.FirstName).ToList()
                        : userResponses.OrderByDescending(u => u.FirstName).ToList();
                case nameof(UserResponse.LastName):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.LastName).ToList()
                        : userResponses.OrderByDescending(u => u.LastName).ToList();
                case nameof(UserResponse.Email):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.Email).ToList()
                        : userResponses.OrderByDescending(u => u.Email).ToList();
                case nameof(UserResponse.UserName):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.UserName).ToList()
                        : userResponses.OrderByDescending(u => u.UserName).ToList();
                case nameof(UserResponse.PhoneNumber):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.PhoneNumber).ToList()
                        : userResponses.OrderByDescending(u => u.PhoneNumber).ToList();
                case nameof(UserResponse.MustChangePassword):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.MustChangePassword).ToList()
                        : userResponses.OrderByDescending(u => u.MustChangePassword).ToList();
                case nameof(UserResponse.IsEnabled):
                    return sortOrder == SortOrderOptions.ASC
                        ? userResponses.OrderBy(u => u.IsEnabled).ToList()
                        : userResponses.OrderByDescending(u => u.IsEnabled).ToList();
                default:
                    throw new ArgumentException($"Invalid property name: {propertyName}");
            }

        }

        public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetUserByIdAsync), nameof(UsersGetterService));

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }

            var user = await _usersRepository.GetUserByIdAsync(userId);
            return user?.ToUserResponse();
        }

        public async Task<UserResponse?> GetUserByNameAsync(string name)
        {
            _logger.LogInformation("{methodName} from {serviceName} has been invoked.", nameof(GetUserByNameAsync), nameof(UsersGetterService));

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Attempted to get user by name, but the provided name was null or whitespace.");
                return null;
            }

            var user = await _usersRepository.GetUserByNameAsync(name);
            return user?.ToUserResponse();
        }
    }
}

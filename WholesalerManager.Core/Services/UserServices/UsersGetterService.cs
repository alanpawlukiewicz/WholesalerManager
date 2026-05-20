using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
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
            var allUsers = await _usersRepository.GetAllUsersAsync();
            return allUsers.Select(usr => usr.ToUserResponse()).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }

            var user = await _usersRepository.GetUserByIdAsync(userId);
            return user?.ToUserResponse();
        }
    }
}

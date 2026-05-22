using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Helpers;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;

namespace WholesalerManager.Core.Services.UserServices
{
    public class UsersRegistrationService : IUsersRegistrationService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly IUsersRepository _usersRepository;
        private readonly IUserNameGeneratorService _userNameGeneratorService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;

        private readonly ILogger<UsersRegistrationService> _logger;

        public UsersRegistrationService(IUsersRepository usersRepository, IUserNameGeneratorService userNameGeneratorService, IPasswordGeneratorService passwordGeneratorService, RoleManager<ApplicationRole> roleManager, ILogger<UsersRegistrationService> logger)
        {
            _usersRepository = usersRepository;
            _userNameGeneratorService = userNameGeneratorService;
            _passwordGeneratorService = passwordGeneratorService;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<UserResponse?> RegisterUserAsync(RegisterDTO registerData)
        {
            ValidationHelper.ModelValidation(registerData);

            bool passwordNotSet = string.IsNullOrWhiteSpace(registerData.Password);

            ApplicationUser user = new ApplicationUser()
            {
                FirstName = registerData.FirstName,
                LastName = registerData.LastName,
                MustChangePassword = passwordNotSet,
                IsEnabled = true,
                Email = registerData.Email,
                UserName = _userNameGeneratorService.Generate(registerData.FirstName, registerData.LastName),
                PhoneNumber = registerData.Phone
            };

            string password = passwordNotSet ? _passwordGeneratorService.Generate() : registerData.Password!;

            var result = await _usersRepository.AddUser(user, password);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user {UserName}. Errors: {Errors}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            // Add role if it doesn't exist
            if (await _roleManager.FindByNameAsync(registerData.UserType.ToString()) == null)
            {
                _logger.LogInformation("Role {RoleName} does not exist. Creating role.", registerData.UserType.ToString());
                await _roleManager.CreateAsync(new ApplicationRole() { Name = registerData.UserType.ToString() });
            }

            // Assign the user to the role
            await _usersRepository.AddNewRoleToUser(user, registerData.UserType.ToString());

            return user.ToUserResponse();
        }
    }
}

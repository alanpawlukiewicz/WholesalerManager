using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersRegistrationService
    {
        Task<UserResponse?> RegisterUserAsync(RegisterDTO registerData);
    }
}

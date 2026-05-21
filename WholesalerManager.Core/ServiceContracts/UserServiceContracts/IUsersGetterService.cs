using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersGetterService
    {
        Task<UserResponse?> GetUserByIdAsync(Guid userId);

        Task<UserResponse?> GetUserByNameAsync(string name);

        Task<List<UserResponse>> GetAllUsersAsync();

        Task<string> GeneratePasswordResetTokenAsync(Guid id);
    }
}

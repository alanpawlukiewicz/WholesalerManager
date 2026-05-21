using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    public interface IUsersGetterService
    {
        Task<UserResponse?> GetUserByIdAsync(Guid userId);

        Task<UserResponse?> GetUserByNameAsync(string name);

        Task<List<UserResponse>> GetAllUsersAsync();

        Task<string> GeneratePasswordResetTokenAsync(Guid id);

        Task<List<UserResponse>> GetFilteredUsers(string? propertyName, string? filter, bool ignoreCase = true);

        Task<List<UserResponse>> GetSortedUsers(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

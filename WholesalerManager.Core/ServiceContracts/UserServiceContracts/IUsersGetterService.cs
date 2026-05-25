using WholesalerManager.Core.DTO.UserDTO;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    /// <summary>
    /// Represents a service contract for retrieving user information and generating password reset tokens.
    /// </summary>
    public interface IUsersGetterService
    {
        /// <summary>
        /// Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the retrieved user response or null if not found.</returns>
        Task<UserResponse?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Asynchronously retrieves a user by their name.
        /// </summary>
        /// <param name="name">The name of the user to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the retrieved user response or null if not found.</returns>
        Task<UserResponse?> GetUserByNameAsync(string name);

        /// <summary>
        /// Asynchronously retrieves all users.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result is a list of all retrieved user responses.</returns>
        Task<List<UserResponse>> GetAllUsersAsync();

        /// <summary>
        /// Creates password reset token for the user with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user for whom to create a password reset token.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the generated password reset token.</returns>
        Task<string> GeneratePasswordResetTokenAsync(Guid id);

        /// <summary>
        /// Asynchronously retrieves a list of users filtered by a specified property and filter value, with an option to ignore case sensitivity.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter by.</param>
        /// <param name="filter">The filter value.</param>
        /// <param name="ignoreCase">A value indicating whether to ignore case sensitivity.</param>
        /// <returns>A task representing the asynchronous operation. The task result is a list of retrieved user responses.</returns>
        Task<List<UserResponse>> GetFilteredUsers(string? propertyName, string? filter, bool ignoreCase = true);

        /// <summary>
        /// Asynchronously retrieves a list of users sorted by a specified property and sort order.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns>A task representing the asynchronous operation. The task result is a list of retrieved user responses.</returns>
        Task<List<UserResponse>> GetSortedUsers(string? propertyName, SortOrderOptions sortOrder = SortOrderOptions.ASC);
    }
}

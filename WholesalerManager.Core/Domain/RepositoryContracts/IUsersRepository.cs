using Microsoft.AspNetCore.Identity;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.Core.Domain.RepositoryContracts
{
    /// <summary>
    /// Represents a repository for managing application users, providing methods for retrieving, updating, adding, and deleting users, as well as managing user roles and password reset tokens.
    /// </summary>
    public interface IUsersRepository
    {
        /// <summary>
        /// Asynchronously retrieves a list of all application users.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of application users.</returns>
        Task<List<ApplicationUser>> GetAllUsersAsync();

        /// <summary>
        /// Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the user if found, otherwise null.</returns>
        Task<ApplicationUser?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the user if found, otherwise null.</returns>
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Asynchronously updates an existing user.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the identity result indicating the success or failure of the operation.</returns>
        Task<IdentityResult> UpdateUser(ApplicationUser user);

        /// <summary>
        /// Asynchronously adds a new user with the specified password.
        /// </summary>
        /// <param name="user">The user to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the identity result indicating the success or failure of the operation.</returns>
        Task<IdentityResult> AddUser(ApplicationUser user, string password);

        /// <summary>
        /// Asynchronously deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the identity result indicating the success or failure of the operation.</returns>
        Task<IdentityResult> DeleteUser(Guid id);

        /// <summary>
        /// Asynchronously adds a new role to a user.
        /// </summary>
        /// <param name="user">The user to add the role to.</param>
        /// <param name="roleName">The name of the role to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the identity result indicating the success or failure of the operation.</returns>
        Task<IdentityResult> AddNewRoleToUser(ApplicationUser user, string roleName);

        /// <summary>
        /// Generates a password reset token for the specified user, which can be used to reset the user's password. This token is typically sent to the user's email address and is valid for a limited time.
        /// </summary>
        /// <param name="user">The user for whom to generate a password reset token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the generated password reset token.</returns>
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

        /// <summary>
        /// Asynchronously retrieves a user by their username.
        /// </summary>
        /// <param name="name">The username of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the user if found, otherwise null.</returns>
        Task<ApplicationUser?> GetUserByNameAsync(string name);
    }
}

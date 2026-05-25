using Microsoft.AspNetCore.Identity;
using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    /// <summary>
    /// Represents a service contract for updating user information, changing passwords, and toggling enabled status in the Wholesaler Manager application.
    /// </summary>
    public interface IUsersUpdaterService
    {
        /// <summary>
        /// Asynchronously updates user information based on the provided UserEditRequest. This method allows for modifying user details such as name, email, and phone number.
        /// </summary>
        /// <param name="userEditRequest">The request containing the user information to be updated.</param>
        /// <returns>A task representing the asynchronous operation. The task result is an IdentityResult indicating the success or failure of the update operation.</returns>
        Task<IdentityResult> UpdateUserAsync(UserEditRequest userEditRequest);

        /// <summary>
        /// Asynchronously changes the password for a user based on their unique identifier (userID).
        /// </summary>
        /// <param name="id">The unique identifier of the user whose password is to be changed.</param>
        /// <returns>A task representing the asynchronous operation. The task result is an IdentityResult indicating the success or failure of the password change operation.</returns>
        Task<IdentityResult> MakeUserChangePassword(Guid id);

        /// <summary>
        /// Asynchronously changes the enabled status of a user based on their unique identifier (userID).
        /// </summary>
        /// <param name="userID">The unique identifier of the user whose enabled status is to be changed.</param>
        /// <returns>A task representing the asynchronous operation. The task result is a boolean value indicating the success or failure of the enabled status change operation.</returns>
        Task<bool> ChangeEnabledStatus(Guid userID);
    }
}

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    /// <summary>
    /// Represents a service contract for deleting users. This interface defines a method for deleting a user based on their unique identifier (userID). Implementing this interface allows for the encapsulation of user deletion logic, ensuring that the operation can be performed consistently across different parts of the application.
    /// </summary>
    public interface IUsersDeleterService
    {
        /// <summary>
        /// Asynchronously deletes user from the system based on their unique identifier (userID). The method returns a boolean value indicating whether the deletion was successful or not. If the user with the specified userID does not exist, the method should return false. If the deletion is successful, it should return true.
        /// </summary>
        /// <param name="userID">The unique identifier of the user to be deleted.</param>
        /// <returns>A task representing the asynchronous operation. The task result is a boolean value indicating whether the deletion was successful.</returns>
        Task<bool> DeleteUser(Guid userID);
    }
}

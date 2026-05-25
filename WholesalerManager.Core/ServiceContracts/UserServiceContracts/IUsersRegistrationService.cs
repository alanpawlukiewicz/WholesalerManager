using WholesalerManager.Core.DTO.UserDTO;

namespace WholesalerManager.Core.ServiceContracts.UserServiceContracts
{
    /// <summary>
    /// Represents a service contract for user registration. This interface defines a method for registering a new user in the system. Implementing this interface allows for the encapsulation of user registration logic, ensuring that the operation can be performed consistently across different parts of the application. The method takes a RegisterDTO object containing the necessary information for user registration and returns a UserResponse object if the registration is successful, or null if it fails (e.g., due to validation errors or if a user with the same email already exists).
    /// </summary>
    public interface IUsersRegistrationService
    {
        /// <summary>
        /// Asynchronously registers a new user in the system based on the provided registration data. The method takes a RegisterDTO object that contains the user's registration information, such as first name, last name, email, phone number, password, and user type. If the registration is successful, it returns a UserResponse object containing the details of the newly registered user. If the registration fails (e.g., due to validation errors or if a user with the same email already exists), it returns null.
        /// </summary>
        /// <param name="registerData">The data required for user registration.</param>
        /// <returns>A task representing the asynchronous operation. The task result is the registered user response or null if registration fails.</returns>
        Task<UserResponse?> RegisterUserAsync(RegisterDTO registerData);
    }
}

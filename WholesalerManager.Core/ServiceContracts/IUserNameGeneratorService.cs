namespace WholesalerManager.Core.ServiceContracts
{
    /// <summary>
    /// Defines a service for generating a username by combining a user's first name, last name, and a random string sequence.
    /// </summary>
    public interface IUserNameGeneratorService
    {
        /// <summary>
        /// Generates a username consisting of a user's first name, last name, and a random string sequence.
        /// </summary>
        /// <param name="firstName">The first name to include in the generated string. Can be null or empty if only the last name should be
        /// used.</param>
        /// <param name="lastName">The last name to include in the generated string. Can be null or empty if only the first name should be
        /// used.</param>
        /// <returns>A formatted string that combines the first name,last name and a random string sequence. If both parameters are null or empty, throws ArgumentNullException.</returns>
        string Generate(string? firstName, string? lastName, int randomPartLength = 6);
    }
}

namespace WholesalerManager.Core.ServiceContracts
{
    /// <summary>
    /// Defines a service for generating random passwords of a specified length.
    /// </summary>
    public interface IPasswordGeneratorService
    {
        /// <summary>
        /// Generates a random string of the specified length.
        /// </summary>
        /// <param name="length">The number of characters in the generated string. Must be greater than zero. The default is 12.</param>
        /// <returns>A randomly generated string with the specified number of characters.</returns>
        string Generate(int length = 12);
    }
}

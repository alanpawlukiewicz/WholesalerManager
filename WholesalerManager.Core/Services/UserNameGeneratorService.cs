using System.Security.Cryptography;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class UserNameGeneratorService : IUserNameGeneratorService
    {
        private const string _chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        public string Generate(string? firstName, string? lastName, int randomPartLength = 6)
        {
            if(string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentNullException("Both first name and last name cannot be null or empty.");
            }

            string cleanFirst = firstName.Replace(" ", "").ToLower();
            string cleanLast = lastName.Replace(" ", "").ToLower();

            
            var randomPart = new char[randomPartLength];
            for (int i = 0; i < randomPartLength; i++)
            {
                randomPart[i] = _chars[RandomNumberGenerator.GetInt32(_chars.Length)];
            }

            return $"{cleanFirst}.{cleanLast}_{new string(randomPart)}";
        }
    }
}

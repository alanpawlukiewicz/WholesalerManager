using System.Security.Cryptography;
using System.Text;
using WholesalerManager.Core.ServiceContracts;

namespace WholesalerManager.Core.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private const string _upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string _lowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string _digits = "0123456789";
        private const string _specials = "!@#$%^&*()_-+=";
        public string Generate(int length = 12)
        {
            var password = new StringBuilder();

            password.Append(_upperCase[RandomNumberGenerator.GetInt32(_upperCase.Length)]);
            password.Append(_lowerCase[RandomNumberGenerator.GetInt32(_lowerCase.Length)]);
            password.Append(_digits[RandomNumberGenerator.GetInt32(_digits.Length)]);
            password.Append(_specials[RandomNumberGenerator.GetInt32(_specials.Length)]);

            for (int i = 4; i < length; i++)
            {
                password.Append(
                    (_upperCase + _lowerCase + _digits + _specials)
                    [RandomNumberGenerator.GetInt32((_upperCase + _lowerCase + _digits + _specials).Length)]
                    );
            }

            return new string(password.ToString().ToCharArray().OrderBy(s => RandomNumberGenerator.GetInt32(100)).ToArray());
        }
    }
}

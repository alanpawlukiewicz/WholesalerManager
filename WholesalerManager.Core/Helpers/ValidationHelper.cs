using System.ComponentModel.DataAnnotations;

namespace WholesalerManager.Core.Helpers
{
    public class ValidationHelper
    {
        internal static void ModelValidation(object? obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);
            if (!isValid)
            {
                throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage, nameof(obj));
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.UserDTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "First name can't be blank.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name can't be blank.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email can't be blank.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public UserTypeOptions UserType { get; set; } = UserTypeOptions.Operator;
    }
}

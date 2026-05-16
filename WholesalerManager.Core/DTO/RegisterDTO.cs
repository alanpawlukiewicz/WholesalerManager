using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO
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

        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public UserTypeOptions UserType { get; set; } = UserTypeOptions.Operator;
    }
}

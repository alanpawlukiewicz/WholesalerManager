using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Enums;

namespace WholesalerManager.Core.DTO.UserDTO
{
    public class UserEditRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name can't be blank.")]
        public string? FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name can't be blank.")]
        public string? LastName { get; set; }
        
        [Required(ErrorMessage = "Username can't be blank.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email can't be blank.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        public ApplicationUser ToApplicationUser()
        {
            return new ApplicationUser
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                Email = Email,
                PhoneNumber = PhoneNumber
            };
        }
    }
}

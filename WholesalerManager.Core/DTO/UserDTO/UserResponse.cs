using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.Core.DTO.UserDTO
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? UserName { get; set; }

        public bool? MustChangePassword { get; set; }

        public bool? IsEnabled { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public UserEditRequest ToUserEditRequest()
        {
            return new UserEditRequest
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

    public static class UserExtensions
    {
        public static UserResponse ToUserResponse(this ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                MustChangePassword = user.MustChangePassword,
                IsEnabled = user.IsEnabled,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace WholesalerManager.Core.DTO.UserDTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool KeepSignedIn { get; set; } = false;
    }
}

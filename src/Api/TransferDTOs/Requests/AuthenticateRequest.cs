using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests
{
    public class AuthenticateRequest
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = String.Empty;
    }
}
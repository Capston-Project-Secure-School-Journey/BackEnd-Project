using System.ComponentModel.DataAnnotations;
using Api.Attributes;

namespace Api.DTOs.User;

public class ResetPasswordDto
{
    [PasswordStrength]
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required] public string Token { get; set; } = string.Empty;
}
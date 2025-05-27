using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.User;

public class SendForgetPasswordEmailDto
{
    [Required(ErrorMessage = "Tên người dùng không được trống.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được trống.")]
    public string Email { get; set; } = string.Empty;
}
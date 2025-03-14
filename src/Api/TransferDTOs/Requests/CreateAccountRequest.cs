using System.ComponentModel.DataAnnotations;
using Api.Attributes;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class CreateAccountRequest
{
    [Range((int)UserType.Driver, (int)UserType.Parent, ErrorMessage = "Tạo account không thành công.")]
    public UserType UserType { get; set; }
    [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên đăng nhập không được quá 100 ký tự.")]
    public string UserName { get; set; } = string.Empty;
    [PasswordStrength]
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Password { get; set; } = string.Empty;
    [RegularExpression(@"^(\+\d{1,2}\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Số điện thoại không đúng.")]
    [MaxLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự.")]
    public string? PhoneNumber { get; set; }
    [MaxLength(200, ErrorMessage = "Email không được quá 200 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Địa chỉ email không đúng.")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Giới tính không được để trống.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Loại giới tính không hợp lệ.")]
    public Gender Gender { get; set; }
}
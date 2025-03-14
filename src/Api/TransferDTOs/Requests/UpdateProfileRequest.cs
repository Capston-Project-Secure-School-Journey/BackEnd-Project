using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class UpdateProfileRequest
{
    [RegularExpression(@"^(\+\d{1,2}\s?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Số điện thoại không đúng.")]
    [MaxLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự.")]
    public string PhoneNumber { get; set; } = string.Empty;
    [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự.")]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(200, ErrorMessage = "Họ không được quá 200 ký tự.")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Giới tính không được để trống.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Loại giới tính không hợp lệ.")]
    public Gender Gender { get; set; }
    [MaxLength(200, ErrorMessage = "Email không được quá 200 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Địa chỉ email không đúng.")]
    public string Email { get; set; } = string.Empty;
    [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng.")]
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(1000, ErrorMessage = "Địa chỉ không được quá 1000 ký tự.")]
    public string? Address { get; set; }
    [MaxLength(1000, ErrorMessage = "Địa chỉ chi tiết không được quá 1000 ký tự.")]
    public string? DetailAddress { get; set; }
}
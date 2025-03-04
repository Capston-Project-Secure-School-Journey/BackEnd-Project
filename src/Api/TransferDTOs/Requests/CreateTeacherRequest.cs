using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class CreateTeacherRequest
{
    [Required(ErrorMessage = "Tên không được để trống.")]
    [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự.")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Họ không được để trống.")]
    [MaxLength(200, ErrorMessage = "Họ không được quá 200 ký tự.")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ngày sinh không được để trống.")]
    [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng.")]
    public DateOnly DateOfBirth { get; set; }
    [Required(ErrorMessage = "Giới tính không được để trống.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Loại giới tính không hợp lệ.")]
    public Gender Gender { get; set; }
    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [DataType(DataType.PhoneNumber, ErrorMessage = "Số điện thoại không đúng.")]
    [MaxLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự.")]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Địa Chỉ email không được để trống.")]
    [MaxLength(200, ErrorMessage = "Email không được quá 200 ký tự.")]
    [DataType(DataType.EmailAddress, ErrorMessage = "Địa chỉ email không đúng.")]
    public string Email { get; set; } = string.Empty;
}
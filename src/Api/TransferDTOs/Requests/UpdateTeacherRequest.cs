using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.Transfers.Requests;

public class UpdateTeacherRequest
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Tên không được để trống.")]
    [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Họ không được để trống.")]
    [MaxLength(200, ErrorMessage = "Họ không được quá 200 ký tự.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Ngày sinh không được để trống.")]
    [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng.")]
    public DateTime DateOfBirth { get; set; }
    [Required(ErrorMessage = "Giới tính không được để trống.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Loại giới tính không hợp lệ.")]
    public Gender Gender { get; set; }
    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [MaxLength(11, ErrorMessage = "Số điện thoại không được quá 11 ký tự.")]
    [DataType(DataType.PhoneNumber, ErrorMessage = "Số điện thoại không đúng.")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Địa Chỉ email không được để trống.")]
    [MaxLength(200, ErrorMessage = "Email không được quá 200 ký tự.")]
    [DataType(DataType.EmailAddress, ErrorMessage = "Địa chỉ email không đúng.")]
    public string Email { get; set; }
}
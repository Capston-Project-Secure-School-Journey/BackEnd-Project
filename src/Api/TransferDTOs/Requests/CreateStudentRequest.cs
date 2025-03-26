using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class CreateStudentRequest
{
    [Required(ErrorMessage = "Tên không được để trống.")]
    [MaxLength(200, ErrorMessage = "Tên không được quá 200 ký tự.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ không được để trống.")]
    [MaxLength(200, ErrorMessage = "Họ không được quá 200 ký tự.")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Date, ErrorMessage = "Ngày sinh không đúng.")]
    [Required(ErrorMessage = "Ngày sinh không được để trống.")]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "Lớp học không được để trống.")]
    public Guid ClassId { get; set; }

    [Required(ErrorMessage = "Giới tính không được để trống.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Loại giới tính không hợp lệ.")]
    public Gender Gender { get; set; }
}
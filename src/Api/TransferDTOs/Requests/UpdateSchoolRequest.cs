using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class UpdateSchoolRequest
{
    [Required(ErrorMessage = "Loại trường học không được để trống.")]
    [EnumDataType(typeof(SchoolType), ErrorMessage = "Loại trường không hợp lệ.")]
    public SchoolType SchoolType { get; set; }

    [Required(ErrorMessage = "Tên trường học không được để trống.")]
    [MaxLength(100, ErrorMessage = "Tên trường học không được quá 100 ký tự.")]
    public string SchoolName { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Mô tả trường học không được quá 2000 ký tự.")]
    public string? SchoolDescription { get; set; }

    [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Địa chỉ phải không được quá 1000 ký tự.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thời gian bắt đầu buổi sáng là bắt buộc.")]
    public TimeSpan MorningStartTime { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc buổi sáng là bắt buộc.")]
    public TimeSpan MorningEndTime { get; set; }

    [Required(ErrorMessage = "Thời gian bắt đầu buổi chiều là bắt buộc.")]
    public TimeSpan AfternoonStartTime { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc buổi chiều là bắt buộc.")]
    public TimeSpan AfternoonEndTime { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }

    [Required]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string PhoneNumber { get; set; } = string.Empty;
}
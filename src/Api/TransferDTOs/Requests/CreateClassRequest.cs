using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Transfers.Requests;

public class CreateClassRequest
{
    [Required(ErrorMessage = "Loại lớp học không được để trống.")]
    [EnumDataType(typeof(Grade), ErrorMessage = "Loại lớp học không hợp lệ.")]
    public Grade Grade { get; set; }
    public List<ManagedTeacher> ManagedTeachers { get; set; }
    [Required(ErrorMessage = "Tên lớp không được để trống.")]
    public string ClassName { get; set; } = string.Empty;
}
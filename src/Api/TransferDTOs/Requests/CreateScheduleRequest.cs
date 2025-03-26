using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class CreateScheduleRequest
{
    [Required]
    [DataType(DataType.Date, ErrorMessage = "Ngày học không đúng.")]
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    [Required(ErrorMessage = "Loại buổi học không được để trống.")]
    [EnumDataType(typeof(SessionType), ErrorMessage = "Loại buổi học không hợp lệ.")]
    public SessionType SessionType { get; set; }
    [Required(ErrorMessage = "Loại lịch học không được để trống.")]
    [EnumDataType(typeof(ScheduleType), ErrorMessage = "Loại lịch học không hợp lệ.")]
    public ScheduleType ScheduleType { get; set; }
    public Guid? ClassId { get; set; }
    public Grade? Grade { get; set; }
}
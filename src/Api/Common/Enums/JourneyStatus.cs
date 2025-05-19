using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum JourneyStatus
{
    [Display(Name = "Chưa bắt đầu")]
    NotStarted,
    [Display(Name = "Đang đón học sinh")]
    InProgress,
    [Display(Name = "Đã hoàn thành")]
    Completed,
    [Display(Name = "Đã hủy")]
    Cancelled,
}
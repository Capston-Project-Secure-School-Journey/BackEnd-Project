using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum SessionType
{
    [Display(Name = "Buổi Sáng")]
    Morning = 0,
    [Display(Name = "Buổi Chiều")]
    Afternoon = 1,
    [Display(Name = "Cả ngày")]
    FullDay = 2
}
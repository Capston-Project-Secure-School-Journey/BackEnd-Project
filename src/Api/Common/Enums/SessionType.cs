using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum SessionType
{
    [Display(Name = "Sáng")]
    Morning = 0,
    [Display(Name = "Chiều")]
    Afternoon = 1,
    [Display(Name = "Cả ngày")]
    FullDay = 2
}
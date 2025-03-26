using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum ScheduleType
{
    [Display(Name = "Học theo lớp")] Class,
    [Display(Name = "Học theo khối")] Grade,
    [Display(Name = "Học toàn trường")] School
}
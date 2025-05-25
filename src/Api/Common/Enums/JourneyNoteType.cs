using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum JourneyNoteType
{
    [Display(Name = "Học sinh hôm nay nghỉ, không cần đón")]
    AbsentToday,
    [Display(Name = "Học sinh đi cùng phụ huynh, không cần đón")]
    GoingWithParent,
    [Display(Name = "Dặn dò đặc biệt")] SpecialInstruction,
    [Display(Name = "Khác")] Other
}
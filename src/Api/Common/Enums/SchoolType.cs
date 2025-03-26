using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum SchoolType
{
    [Display(Name = "Mầm non")] Preschool,

    [Display(Name = "Tiểu học")] PrimarySchool,

    [Display(Name = "Trung học cơ sở")] MiddleSchool,

    [Display(Name = "Trung học phổ thông")]
    HighSchool
}
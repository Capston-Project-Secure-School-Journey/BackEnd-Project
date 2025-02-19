
using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum Gender
{
    [Display(Name = "Nam")]
    Female,
    [Display(Name = "Nữ")]
    Male
}
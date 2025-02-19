using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum Grade
{
    [Display(Name = "Lớp Mầm")]
    Daycare,
    [Display(Name = "Lớp Chồi")]
    JuniorKindergarten,
    [Display(Name = "Lớp Lá")]
    SeniorKindergarten,
    [Display(Name = "Lớp Một")]
    Grade1,
    [Display(Name = "Lớp Hai")]
    Grade2,
    [Display(Name = "Lớp Ba")]
    Grade3,
    [Display(Name = "Lớp Bốn")]
    Grade4,
    [Display(Name = "Lớp Năm")]
    Grade5,
    [Display(Name = "Lớp Sáu")]
    Grade6,
    [Display(Name = "Lớp Bảy")]
    Grade7,
    [Display(Name = "Lớp Tám")]
    Grade8,
    [Display(Name = "Lớp Chín")]
    Grade9,
    [Display(Name = "Lớp Mười")]
    Grade10,
    [Display(Name = "Lớp Mười Một")]
    Grade11,
    [Display(Name = "Lớp Mười Hai")]
    Grade12
}
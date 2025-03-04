using System.ComponentModel.DataAnnotations;

namespace Api.Common.Enums;

public enum Relationship
{
    [Display(Name = "Cha, Mẹ")]
    Parent, 
    [Display(Name = "Anh, Chị")]
    OlderSibling,
    [Display(Name = "Ông Bà")]
    Grandparent,
    [Display(Name = "Cô, Chú, Bác, Dì")]
    Uncle,
    [Display(Name = "Người giám hộ")]
    Guardian
}
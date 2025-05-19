using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class SkipStudentRequest
{
    [Required(ErrorMessage = "Lí do bỏ qua việc đón học sinh không được để trống.")]
    public string Reason { get; set; } = string.Empty;
    [Required(ErrorMessage = "Học sinh cần bỏ qua không được để trống.")]
    public Guid StudentId { get; set; }
}
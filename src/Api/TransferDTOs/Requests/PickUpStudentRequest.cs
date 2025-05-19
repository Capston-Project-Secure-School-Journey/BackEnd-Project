using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class PickUpStudentRequest
{
    [Required(ErrorMessage = "Mã code không thể rỗng.")]
    public string SecretCode { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class RejectApplicationRequest
{
    [Required(ErrorMessage="Lí do từ chối không đuợc để trống")]
    public string Reason { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class CancelApplicationRequest
{
    [Required(ErrorMessage="Lí do hủy hồ sơ không đuợc để trống")]
    public string Reason { get; set; } = null!;
}
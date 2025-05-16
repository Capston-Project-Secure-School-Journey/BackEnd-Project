using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class RequestMoreInfoRequest
{
    [Required(ErrorMessage="Thông tin cần bổ sung không được để trống.")]
    public string Reason { get; set; } = null!;
}
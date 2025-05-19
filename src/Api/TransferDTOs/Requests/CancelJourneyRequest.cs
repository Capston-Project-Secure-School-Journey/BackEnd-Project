using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class CancelJourneyRequest
{
    [Required(ErrorMessage = "Lí do hủy chuyến không được để trống.")]
    public string Reason { get; set; } = string.Empty;
}
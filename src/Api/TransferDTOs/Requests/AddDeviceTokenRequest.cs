using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class AddDeviceTokenRequest
{
    [Required]
    public string DeviceToken { get; set; } = string.Empty;
}
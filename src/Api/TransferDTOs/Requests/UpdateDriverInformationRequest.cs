using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class UpdateDriverInformationRequest
{
    public string VehicleType { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    [Range(4, 200, ErrorMessage = "Số ghế ngồi phải lớn hơn 3")]
    public int SeatingCapacity { get; set; }
    public List<(Guid, DriverInformationImageType)> DriverInformationImages { get; set; } = [];
    public List<Guid> VehicleImages { get; set; } = [];
}

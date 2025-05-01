using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }          
    public DateTime RequestedDate { get; set; }
    public string MotivationLetter { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public RequestStatus RequestStatus { get; set; } 
    public Guid? ApprovedBy { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public DateTime? LastCheckDrivingLicense { get; set; }
    public List<DriverInformationImageUrl> DriverInformationImages { get; set; } = [];
    public List<string> VehicleImages { get; set; } = [];
    public List<DriverRequestStatusHistoryResponse> DriverRequestStatusHistoryResponse { get; set; } = [];
}

public class DriverRequestStatusHistoryResponse
{
    public Guid Id { get; set; }
    public RequestStatus? FromStatus { get; set; }
    public RequestStatus ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Note { get; set; } = string.Empty;
}
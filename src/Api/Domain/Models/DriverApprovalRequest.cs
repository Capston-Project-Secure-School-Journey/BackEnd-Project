using Api.Common.Enums;

namespace Api.Domain.Models;

public class DriverApprovalRequest
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
    public List<DriverInformationImage> DriverInformationImages { get; set; }
    public List<FileMetadata> VehicleImages { get; set; }
    public Driver Driver { get; set; } = null!;
    public HashSet<DriverRequestStatusHistory> DriverRequestStatusHistories { get; set; }

    public DriverApprovalRequest()
    {
        DriverRequestStatusHistories = [];
        DriverInformationImages = [];
        VehicleImages = [];
    }
}
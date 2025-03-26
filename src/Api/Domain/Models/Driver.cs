using Api.Common.Enums;

namespace Api.Domain.Models;

public class Driver : User
{
    public VehicleType? VehicleType { get; set; }
    public string? LicenseNumber { get; set; }
    public List<VerifiedBy> VerifiedBy { get; set; }
    public DateTimeOffset? LastCheckDrivingLicense { get; set; }
    public List<FileMetadata> DriverInformationImage { get; set; }

    public Driver()
    {
        DriverInformationImage = [];
        VerifiedBy = [];
    }
}

public class VerifiedBy
{
    public Guid SchoolId { get; set; }
    public DateTime VerifiedAt { get; set; }
}
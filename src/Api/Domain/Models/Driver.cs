using Api.Common.Enums;

namespace Api.Domain.Models;

public class Driver: User
{
    public VehicleType VehicleType { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsVerify { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public DateTimeOffset? LastCheckDrivingLicense { get; set; }
    public string DriverInformationImage { get; set; } = string.Empty;
}
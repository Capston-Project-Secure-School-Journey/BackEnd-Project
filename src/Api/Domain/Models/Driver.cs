using Api.Common.Enums;

namespace Api.Domain.Models;

public class Driver : User
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string VehicleType { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public List<VerifiedBy> VerifiedBy { get; set; }
    public DateTime? LastCheckDrivingLicense { get; set; }
    public List<DriverInformationImage> DriverInformationImages { get; set; }
    public List<FileMetadata>  VehicleImages { get; set; }

    public Driver()
    {
        DriverInformationImages = [];
        VehicleImages = [];
        VerifiedBy = [];
    }
}

public class VerifiedBy
{
    public Guid SchoolId { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class DriverInformationImage : FileMetadata
{
    public DriverInformationImageType Type { get; set; }
}
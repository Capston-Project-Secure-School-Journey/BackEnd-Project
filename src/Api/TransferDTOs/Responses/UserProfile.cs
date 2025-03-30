using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.TransferDTOs.Responses;

public class UserProfile
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string UserTypeName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string DetailAddress { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public AccountStatus AccountStatus { get; set; }
    public VerificationMethod? VerificationMethod { get; set; }
    public Guid? SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    
    // driver
    public string VehicleType { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public List<VerifiedBy> VerifiedBy { get; set; } = [];
    public DateTime? LastCheckDrivingLicense { get; set; }
    public List<string> VehicleImages { get; set; } = [];
    public List<(string, DriverInformationImageType)> DriverInformationImages { get; set; } = [];
    public List<Guid> VehicleImageKeys { get; set; } = [];
    public List<(Guid, DriverInformationImageType)> DriverInformationImageKeys { get; set; } = [];
}
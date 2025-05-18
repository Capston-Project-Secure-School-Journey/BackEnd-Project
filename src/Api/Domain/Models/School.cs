using Api.Common.Enums;

namespace Api.Domain.Models;

public class School : BaseModel
{
    public Guid Id { get; set; }
    public SchoolType SchoolType { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolDescription { get; set; }
    public string Address { get; set; } = string.Empty;
    public double AddressLat { get; set; }
    public double AddressLng { get; set; }
    public TimeSpan MorningStartTime { get; set; }
    public TimeSpan MorningEndTime { get; set; }
    public TimeSpan AfternoonEndTime { get; set; }
    public TimeSpan AfternoonStartTime { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public List<FileMetadata> Images { get; set; }
    public HashSet<ClassSchedule> ClassSchedules { get; set; }
    public HashSet<SchoolPerson> SchoolPersons { get; set; }

    public School()
    {
        Images = [];
        ClassSchedules = new HashSet<ClassSchedule>();
        SchoolPersons = new HashSet<SchoolPerson>();
    }
}
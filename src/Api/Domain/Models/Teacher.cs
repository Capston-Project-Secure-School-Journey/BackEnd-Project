using Api.Common.Enums;

namespace Api.Domain.Models;

public class Teacher : BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Guid? AvatarKey { get; set; }

    public School School { get; set; } = null!;
    public HashSet<Class> ManagedClasses { get; set; }

    public Teacher()
    {
        ManagedClasses = new HashSet<Class>();
    }
}
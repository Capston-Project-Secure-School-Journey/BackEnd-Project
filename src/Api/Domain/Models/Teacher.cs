using Api.Common.Enums;

namespace Api.Domain.Models;

public class Teacher: BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public Guid? AvatarKey { get; set; }
    
    public virtual School School { get; set; }
    public virtual HashSet<Class> ManagedClasses { get; set; }
}
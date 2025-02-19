using Api.Common.Enums;

namespace Api.Domain.Models;

public class Student: BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public Gender Gender { get; set; }
    public Guid? AvatarKey { get; set; }
    public Guid? QrImageKey { get; set; }
    public string PickUpLocation { get; set; } = string.Empty;
    public decimal PickUpLat { get; set; }
    public decimal PickUpLng { get; set; }
    public int? LocationGroup { get; set; }
    public List<ManagedBy> ManagedBy { get; set; }
    
    public virtual School School { get; set; }
    public virtual Class Class { get; set; }
}

public class ManagedBy
{
    public Guid ParentId { get; set; }
    public Relationship RelationshipWithStudent { get; set; }
}
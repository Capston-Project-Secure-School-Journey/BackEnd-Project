using Api.Common.Enums;

namespace Api.Domain.Models;

public class StudentOnBus
{
    public Guid StudentId { get; set; }
    public List<ParentInfo> Parents { get; set; } = [];
    public string PickupAddress { get; set; } = string.Empty;
    public double PickupLat { get; set; }
    public double PickupLng { get; set; }
    public Gender Gender { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsPickedUp { get; set; } = false;
    public DateTimeOffset? PickedUpTime { get; set; } = null;
    public bool IsDroppedOff { get; set; } = false;
    public DateTimeOffset? DroppedOffTime { get; set; } = null;
    public bool SkipPickup { get; set; } = false;
    public string IsSkipUpReason { get; set; } = string.Empty;
}

public class ParentInfo
{
    public Guid ParentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Relationship Relationship { get; set; }
}
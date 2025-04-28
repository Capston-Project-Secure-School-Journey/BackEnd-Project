using Api.Common.Enums;

namespace Api.Domain.Models;

public class StudentOnBus
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public decimal PickupLat { get; set; }
    public decimal PickupLng { get; set; }
    public Gender Gender { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsPickedUp { get; set; } = false;
    public DateTime? PickedUpTime { get; set; } = null;
    public bool IsDroppedOff { get; set; } = false;
    public DateTime? DroppedOffTime { get; set; } = null;
    public bool SkipPickup { get; set; } = false;
}
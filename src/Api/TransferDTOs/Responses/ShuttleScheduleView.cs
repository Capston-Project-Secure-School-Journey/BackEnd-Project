using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ShuttleScheduleView
{
    public Dictionary<DateOnly, List<ShuttleScheduleDateView>> Data { get; set; } = [];
}

public class ShuttleScheduleDateView
{
    public DateOnly Date { get; set; }
    public Guid SchoolId { get; set; }
    public SessionType SessionType { get; set; }
    public int NumberOfStudents { get; set; }
    public int NumberOfTrips { get; set; }
}

public class GroupKey
{
    public DateOnly Date { get; set; }
    public Guid SchoolId { get; set; }
    public SessionType SessionType { get; set; }
}

public class ShuttleScheduleGroupResult
{
    public GroupKey Id { get; set; } = null!;
    public int TotalStudents { get; set; }
    public int TotalTrips { get; set; }
}
using Api.Common.Enums;

namespace Api.TransferDTOs.Responses;

public class ClassScheduleResponse
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Grade? Grade { get; set; }
}

public class ClassScheduleResponseView
{
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid? ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Grade? Grade { get; set; }
}

public class ClassSchedulePaginationResponse
{
    public Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>> ClassSchedules { get; set; } = new Dictionary<DateOnly, IEnumerable<ClassScheduleResponseView>>();
}
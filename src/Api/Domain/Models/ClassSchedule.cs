using Api.Common.Enums;

namespace Api.Domain.Models;

public class ClassSchedule : BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid ClassId { get; set; }
    public Grade? Grade { get; set; }

    public Guid? ScheduleGroupId { get; set; }
    
    public ScheduleGroup? ScheduleGroup { get; set; }
    public School School { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
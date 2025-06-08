using Api.Common.Enums;

namespace Api.Domain.Models;

public class ScheduleGroup : BaseModel, ICloneable
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Grade? Grade { get; set; }
    public List<Guid> ClassException { get; set; }

    public School School { get; set; } = null!;
    public HashSet<ClassSchedule> ClassSchedules { get; set; }

    public ScheduleGroup()
    {
        ClassException = new List<Guid>();
        ClassSchedules = new HashSet<ClassSchedule>();
    }

    public object Clone()
    {
        var scheduleGroup = new ScheduleGroup()
        {
            Id = Guid.NewGuid(),
            SchoolId = SchoolId,
            Date = Date,
            SessionType = SessionType,
            ScheduleType = ScheduleType,
            Grade = Grade,
            ClassException = ClassException,
            ClassSchedules = ClassSchedules.Select(cs => cs.Clone() as ClassSchedule).ToHashSet()!
        };

        foreach (var classSchedule in scheduleGroup.ClassSchedules)
        {
            classSchedule.ScheduleGroupId = scheduleGroup.Id;
        }

        return scheduleGroup;
    }
}
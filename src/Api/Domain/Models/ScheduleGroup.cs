using Api.Common.Enums;

namespace Api.Domain.Models;

public class ScheduleGroup : BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public DateOnly Date { get; set; }
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Grade? Grade { get; set; }
    public List<Guid> ClassException { get; set; }

    public virtual School School { get; set; } = null!;

    public ScheduleGroup()
    {
        ClassException = new List<Guid>();
    }
}
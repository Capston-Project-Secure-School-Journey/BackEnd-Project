using Api.Common.Enums;

namespace Api.DTOs.ScheduleManagement;

public class CreateScheduleDto
{
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public Guid? ClassId { get; set; }
    public Grade? Grade { get; set; }
    public List<Guid> ClassException { get; set; } = [];
}
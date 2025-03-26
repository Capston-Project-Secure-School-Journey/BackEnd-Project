using Api.Common.Enums;

namespace Api.DTOs.ScheduleManagement;

public class UpdateScheduleDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public SessionType SessionType { get; set; }
    public Guid ClassId { get; set; }
}
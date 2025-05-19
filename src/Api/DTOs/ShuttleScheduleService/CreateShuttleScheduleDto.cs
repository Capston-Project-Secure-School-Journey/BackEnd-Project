using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.DTOs.ShuttleScheduleService;

public class CreateShuttleScheduleDto
{
    public Guid DriverId { get; set; }
    public List<Student> Students { get; set; } = [];
    public Guid SchoolId { get; set; }
    public SessionType SessionType { get; set; }
    public ShuttleScheduleType Type { get; set; }
    public DateOnly Date { get; set; }
}
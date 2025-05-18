using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.DTOs.PickupScheduleService;

public class CreatePickupScheduleDto
{
    public Guid DriverId { get; set; }
    public List<Student> Students { get; set; } = [];
    public Guid SchoolId { get; set; }
    public SessionType SessionType { get; set; }
    public PickupScheduleType Type { get; set; }
    public DateOnly Date { get; set; }
}
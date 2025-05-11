using Api.Domain.Models;
using Api.DTOs.PickupScheduleService;

namespace Api.Services.PickupScheduleService;

public interface IPickupScheduleService
{
    Task<PickupSchedule> AddPickupSchedule(CreatePickupScheduleServiceDto request);
}
using Api.Domain.Models;
using Api.DTOs.PickupScheduleService;
using Api.TransferDTOs.Responses;

namespace Api.Services.PickupScheduleService;

public interface IPickupScheduleService
{
    Task<PickupSchedule> AddPickupSchedule(CreatePickupScheduleDto request);
    Task<List<PickupSchedule>> AddPickupSchedule(List<CreatePickupScheduleDto> requests);
    Task<PickupScheduleView> GetPickupScheduleView(DateOnly date, Guid schoolId);
    Task<List<PickupScheduleResponse>> GetPickupScheduleByDate(DateOnly date, Guid schoolId);
    Task<PickupSchedule> GetPickupSchedule(Guid pickupScheduleId);
    Task IsOwnerOfPickupSchedule(Guid pickupScheduleId, Guid schoolId);
}
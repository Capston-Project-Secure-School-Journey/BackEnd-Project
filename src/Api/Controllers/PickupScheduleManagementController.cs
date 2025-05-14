using Api.Attributes;
using Api.Common.Enums;
using Api.Domain.Models;
using Api.Services.PickupScheduleService;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("pickup-schedules")]
public class PickupScheduleManagementController(IPickupScheduleService pickupScheduleService) : ControllerBase
{
    [HttpGet("pickup-schedule-view")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<PickupScheduleView> GetPickupScheduleView([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await pickupScheduleService.GetPickupScheduleView(date, schoolId);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<List<PickupScheduleResponse>> GetPickupScheduleByDate([FromQuery] DateOnly date)
    {
        var schoolId = this.GetSchoolId();
        return await pickupScheduleService.GetPickupScheduleByDate(date, schoolId);
    }
    
    [HttpGet("{pickupScheduleId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<PickupSchedule> GetPickupSchedule([FromRoute] Guid pickupScheduleId)
    {
        var schoolId = this.GetSchoolId();
        await pickupScheduleService.IsOwnerOfPickupSchedule(pickupScheduleId, schoolId);
        return await pickupScheduleService.GetPickupSchedule(pickupScheduleId);
    }
}
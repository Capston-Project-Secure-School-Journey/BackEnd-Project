using Api.Attributes;
using Api.Common.Utilities;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Extensions;
using Api.Services.DriverSchoolTripService;
using Api.Services.NotificationService;
using Api.Services.ScanDeviceSchoolTripService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("testing")]
public class TestController(
    INotificationSender notificationSender,
    INotificationService notificationService,
    IDriverSchoolTripHandler handler,
    IScanDeviceSchoolTripHandler scanDeviceSchoolTripHandler)
    : ControllerBase
{
    public static Guid DriverId;

    [HttpPost("send-notification")]
    [ValidateModel]
    [Authorize()]
    public async Task<ActionResult> UploadAvatar(
        [FromForm] Guid userId,
        [FromForm] string deviceToken,
        [FromForm] string title,
        [FromForm] string content
    )
    {
        var createNotificationDto = new CreateNotificationDto()
        {
            Title = title,
            Content = content,
            RecipientId = userId,
            Navigation = string.Empty
        };
        var notification = await notificationService.CreateNotification(createNotificationDto);
        await notificationSender.SendOneAsync(deviceToken, title, content, null);
        return Ok(notification);
    }

    [HttpPut("set-up-testing")]
    public IActionResult SettingTest([FromForm] bool isTesting,
        [FromForm] DateTime dateTime,
        Guid driverId)
    {
        DriverId = driverId;
        DateTimeHelper.Setup(isTesting);
        DateTimeHelper.TestTime(dateTime);
        return Ok();
    }

    [HttpGet("driver/school-trips")]
    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate([FromQuery] DateOnly date)
    {
        return await handler.GetShuttleScheduleByDate(DriverId, date);
    }

    [HttpGet("driver/school-trips/{shuttleScheduleId}")]
    public async Task<ShuttleSchedule> GetShuttleSchedule([FromRoute] Guid shuttleScheduleId)
    {
        return await handler.GetShuttleSchedule(shuttleScheduleId, DriverId);
    }

    [HttpPut("driver/school-trips/{shuttleScheduleId}/start-journey")]
    public async Task StartJourney([FromRoute] Guid shuttleScheduleId)
    {
        await handler.StartJourney(shuttleScheduleId, DriverId);
    }

    [HttpPut("driver/school-trips/{shuttleScheduleId}/end-journey")]
    public async Task EndJourney([FromRoute] Guid shuttleScheduleId)
    {
        await handler.EndJourney(shuttleScheduleId, DriverId);
    }

    [HttpPut("driver/school-trips/{shuttleScheduleId}/cancel-journey")]
    public async Task CancelJourney([FromRoute] Guid shuttleScheduleId, [FromBody] CancelJourneyRequest request)
    {
        await handler.CancelJourney(shuttleScheduleId, DriverId, request.Reason);
    }

    [HttpPut("driver/school-trips/{shuttleScheduleId}/skip-student")]
    public async Task SkipStudent([FromRoute] Guid shuttleScheduleId, [FromBody] SkipStudentRequest request)
    {
        await handler.SkipStudent(shuttleScheduleId, DriverId, request.StudentId, request.Reason);
    }

    [HttpGet("driver/school-trips/has-in-progress-shuttle")]
    public async Task<bool> HasInProgressShuttle()
    {
        return await handler.HasInProgressShuttle(DriverId);
    }

    [HttpGet("driver/school-trips/current-shuttle")]
    public async Task<ShuttleSchedule> GetCurrentShuttleScheduleByDriver()
    {
        return await handler.GetCurrentShuttleScheduleByDriver(DriverId);
    }

    [HttpGet("driver/school-trips/has-up-coming-shuttle")]
    public async Task<bool> HasUpcomingShuttle()
    {
        return await handler.HasUpcomingShuttle(DriverId);
    }

    [HttpGet("driver/school-trips/up-coming-shuttle")]
    public async Task<ShuttleSchedule> GetUpcomingShuttleSchedule()
    {
        return await handler.GetUpcomingShuttleSchedule(DriverId);
    }

    [HttpPut("driver/school-trips/{shuttleScheduleId}/update-current-location")]
    public async Task UpdateCurrentLocation([FromRoute] Guid shuttleScheduleId,
        [FromBody] UpdateCurrentLocationRequest request)
    {
        await handler.UpdateCurrentAddress(shuttleScheduleId, DriverId, request.Latitude, request.Longitude);
    }

    [HttpPut("scan-device/school-trips/pick-up")]
    public async Task PickUpStudent([FromBody] Guid studentId)
    {
        var hash = HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(studentId));
        await scanDeviceSchoolTripHandler.PickUpStudent(hash);
    }

    [HttpPut("scan-device/school-trips/drop-off")]
    public async Task DropOffStudent([FromBody] Guid studentId)
    {
        var hash = HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(studentId));

        await scanDeviceSchoolTripHandler.DropOffStudent(hash);
    }
}
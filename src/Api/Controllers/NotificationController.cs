using Api.Attributes;
using Api.Domain.Models;
using Api.Services.NotificationService;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    [Authorize()]
    public async Task<Pagination<Notification>> GetMyNotifications([FromQuery] int currentPage = 0)
    {
        var userId = this.GetUserId();
        return await notificationService.GetNotifications(userId, currentPage);
    }

    [HttpGet("unread-count")]
    [Authorize()]
    public async Task<int> GetUnreadNotificationCount()
    {
        var userId = this.GetUserId();
        return await notificationService.NumberOfNotReadNotification(userId);
    }

    [HttpGet("{notificationId}")]
    [Authorize()]
    public async Task<Notification> GetNotification([FromRoute] Guid notificationId)
    {
        var userId = this.GetUserId();
        await notificationService.IsOwnerOfNotification(notificationId, userId);
        return await notificationService.GetNotificationAsync(notificationId);
    }

    [HttpPut("mark-read")]
    [Authorize()]
    public async Task<ActionResult> MarkNotificationByRecipient()
    {
        var userId = this.GetUserId();
        await notificationService.MarkNotificationByRecipient(userId);
        return Ok();
    }

    [HttpPut("{notificationId}/mark-read")]
    [Authorize()]
    public async Task<ActionResult> MarkNotification([FromRoute] Guid notificationId)
    {
        var userId = this.GetUserId();
        await notificationService.IsOwnerOfNotification(notificationId, userId);
        await notificationService.MarkNotification(notificationId);
        return Ok();
    }
}
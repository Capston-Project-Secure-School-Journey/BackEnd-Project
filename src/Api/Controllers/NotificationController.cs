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
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    [Authorize()]
    public async Task<Pagination<Notification>> GetMyNotifications([FromQuery] int currentPage)
    {
        var userId = this.GetUserId();
        return await _notificationService.GetNotifications(userId, currentPage);
    }

    [HttpGet("unread-count")]
    [Authorize()]
    public async Task<int> GetUnreadNotificationCount()
    {
        var userId = this.GetUserId();
        return await _notificationService.NumberOfNotReadNotification(userId);
    }

    [HttpGet("{notificationId}")]
    [Authorize()]
    public async Task<Notification> GetNotification([FromRoute] Guid notificationId)
    {
        var userId = this.GetUserId();
        await _notificationService.IsOwnerOfNotification(notificationId, userId);
        return await _notificationService.GetNotificationAsync(notificationId);
    }
}
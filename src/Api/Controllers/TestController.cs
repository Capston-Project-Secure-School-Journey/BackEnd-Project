using Api.Attributes;
using Api.DTOs.NotificationService;
using Api.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("testing")]
public class TestController(INotificationSender notificationSender, INotificationService notificationService)
    : ControllerBase
{
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
}
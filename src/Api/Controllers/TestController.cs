using Api.Attributes;
using Api.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("testing")]
public class TestController(INotificationSender notificationSender) : ControllerBase
{
    private readonly INotificationSender _notificationSender = notificationSender;

    [HttpPost("send-notification")]
    [ValidateModel]
    [Authorize()]
    public async Task<ActionResult> UploadAvatar(
        [FromForm] string deviceToken,
        [FromForm] string title,
        [FromForm] string content
    )
    {
        await _notificationSender.SendAsync(deviceToken, title, content);
        return Ok();
    }
}
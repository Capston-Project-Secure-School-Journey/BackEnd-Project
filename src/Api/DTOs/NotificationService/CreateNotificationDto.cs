namespace Api.DTOs.NotificationService;

public class CreateNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Type { get; set; } = 0;
    public Guid RecipientId { get; set; }
    public string Navigation { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
}
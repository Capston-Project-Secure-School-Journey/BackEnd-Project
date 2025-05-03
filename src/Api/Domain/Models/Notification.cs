namespace Api.Domain.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Type { get; set; } = 0;
    public Guid RecipientId { get; set; }
    public Guid? SenderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; } = false;
    public string Navigation { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public User Recipient { get; set; } = null!;
    public User? Sender { get; set; }
}
namespace Api.Domain.Models;

public class BaseModel
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public bool IsDeleted { get; set; } = false;

}
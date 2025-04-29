namespace Api.Domain.Models;

public class SystemVariable
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
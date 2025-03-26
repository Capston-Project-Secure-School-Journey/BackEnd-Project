namespace Api.Domain.Models;

public class SchoolPerson : User
{
    public Guid SchoolId { get; set; }
    public School School { get; set; } = null!;
}
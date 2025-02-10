namespace Api.Domain.Models;

public class SchoolPerson: User
{
    public Guid SchoolId { get; set; }
    public virtual School School { get; set; }
}
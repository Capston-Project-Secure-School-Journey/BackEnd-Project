using Api.Common.Enums;

namespace Api.Domain.Models;

public class Class: BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public List<ManagerTeacher> ManagerTeacher { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int NumberOfStudent { get; set; }
    
    public virtual School School { get; set; }
}
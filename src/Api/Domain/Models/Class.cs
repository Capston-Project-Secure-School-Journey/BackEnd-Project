using Api.Common.Enums;

namespace Api.Domain.Models;

public class Class : BaseModel
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Grade Grade { get; set; }
    public List<ManagedTeacher> ManagedTeachers { get; set; } = null!;
    public string ClassName { get; set; } = string.Empty;
    public int NumberOfStudent { get; set; }

    public School School { get; set; } = null!;
    public HashSet<Student> Students { get; set; }

    public Class()
    {
        Students = new HashSet<Student>();
    }
}
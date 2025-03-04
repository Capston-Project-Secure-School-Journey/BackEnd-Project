namespace Api.DTOs.ChildrenManagement;

public class ChildDto
{
    public Guid Id { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
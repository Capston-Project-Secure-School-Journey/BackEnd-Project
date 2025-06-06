namespace Api.TransferDTOs.Requests;

public class GetStudentRequest : QueryTemplate
{
    public Guid? StudentId { get; set; } = null;
    public string? Name { get; set; } = null;
    public Guid? ClassId { get; set; } = null;

    public string? ClassName { get; set; } = null;
}
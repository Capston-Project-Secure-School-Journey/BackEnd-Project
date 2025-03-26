namespace Api.TransferDTOs.Requests;

public class GetStudentRequest : QueryTemplate
{
    public string Name { get; set; } = string.Empty;
    public Guid? ClassId { get; set; } = null;
}
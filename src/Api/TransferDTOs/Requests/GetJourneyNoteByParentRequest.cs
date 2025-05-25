namespace Api.TransferDTOs.Requests;

public class GetJourneyNoteByParentRequest : QueryTemplate
{
    public Guid? ShuttleId { get; set; }
}
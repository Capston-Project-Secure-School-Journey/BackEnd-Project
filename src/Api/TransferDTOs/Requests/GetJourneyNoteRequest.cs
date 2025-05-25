using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class GetJourneyNoteRequest : QueryTemplate
{
    public Guid? ShuttleId { get; set; }
}
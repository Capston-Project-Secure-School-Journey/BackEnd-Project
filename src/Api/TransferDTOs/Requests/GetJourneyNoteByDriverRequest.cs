using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class GetJourneyNoteByDriverRequest : QueryTemplate
{
    [Required(ErrorMessage = "Mã hành trình không thể trống.")]
    public Guid ShuttleId { get; set; }
}
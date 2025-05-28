using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class ReadAllJourneyNoteRequest
{
    [Required(ErrorMessage = "Mã hành trình không thể trống.")]
    public Guid JourneyId { get; set; }
}
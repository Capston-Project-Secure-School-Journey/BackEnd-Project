using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class CloneDayScheduleRequest
{
    [Required]
    public DateOnly DateSource { get; set; }
    [Required]
    public DateOnly DateDestination { get; set; }
}
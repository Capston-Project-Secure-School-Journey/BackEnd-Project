using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class CloneWeekScheduleRequest
{
    [Required]
    public DateOnly WeekSource { get; set; }
    [Required]
    public DateOnly WeekDestination { get; set; }
}
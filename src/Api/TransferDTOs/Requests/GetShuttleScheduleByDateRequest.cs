using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class GetShuttleScheduleByDateRequest : QueryTemplate
{
    [Required(ErrorMessage = "Ngày học là bắt buộc.")]
    public DateOnly Date { get; set; }

    public SessionType? SessionType { get; set; } = null;
    public ShuttleScheduleType? ShuttleScheduleType { get; set; } = null;
    public Guid? DriverId { get; set; } = null;
    public string? DriverName { get; set; } = null;
    public JourneyStatus? JourneyStatus { get; set; } = null;
}
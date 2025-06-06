using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;

namespace Api.TransferDTOs.Requests;

public class GetScheduleByDateRequest : QueryTemplate
{
    [Required(ErrorMessage = "Ngày học là bắt buộc.")]
    public DateOnly Date { get; set; }

    public SessionType? SessionType { get; set; } = null;
    public Guid? ClassId { get; set; } = null;
    public string? ClassName { get; set; } = null;
    public Grade? Grade { get; set; } = null;
}
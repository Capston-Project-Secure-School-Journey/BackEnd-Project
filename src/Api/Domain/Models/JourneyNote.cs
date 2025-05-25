using Api.Common.Enums;

namespace Api.Domain.Models;

public class JourneyNote
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid JourneyId { get; set; }
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime RequestedDate { get; set; }
    public JourneyNoteType Type { get; set; }
    public bool IsReadByDriver { get; set; } = false;
}
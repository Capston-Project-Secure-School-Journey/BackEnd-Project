using System.Text.Json.Serialization;
using NSwag.Annotations;

namespace Api.DTOs.JourneyNoteService;

public class UpdateJourneyNoteDto
{
    [JsonIgnore] [OpenApiIgnore] public Guid JourneyNoteId { get; set; }
    public string Description { get; set; } = string.Empty;
}
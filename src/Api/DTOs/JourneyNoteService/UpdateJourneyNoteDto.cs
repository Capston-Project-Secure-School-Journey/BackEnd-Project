using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.DTOs.JourneyNoteService;

public class UpdateJourneyNoteDto
{
    [JsonIgnore] [SwaggerIgnore] public Guid JourneyNoteId { get; set; }
    public string Description { get; set; } = string.Empty;
}
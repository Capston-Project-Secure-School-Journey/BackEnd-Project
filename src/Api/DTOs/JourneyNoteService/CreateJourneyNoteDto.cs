using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Api.Common.Enums;
using NSwag.Annotations;

namespace Api.DTOs.JourneyNoteService;

public class CreateJourneyNoteDto
{
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã chuyến đi không được để trống.")]
    public Guid JourneyId { get; set; }

    [Required(ErrorMessage = "Mã học sinh không được để trống.")]
    public Guid StudentId { get; set; }

    [OpenApiIgnore] 
    [JsonIgnore]
    public Guid ParentId { get; set; }

    [Required(ErrorMessage = "Loại ghi chú không được trống.")]
    [EnumDataType(typeof(JourneyNoteType), ErrorMessage = "Loại ghi chú không hợp lệ.")]
    public JourneyNoteType Type { get; set; }
}
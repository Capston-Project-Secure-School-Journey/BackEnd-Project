using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.ChildrenManagement;

public class UpdateChildPickupLocationDto
{
    public Guid ChildId { get; set; }

    [Required]
    [MaxLength(1000, ErrorMessage = "Địa chỉ không quá 1000 không được quá 1000 ký tự.")]
    public string PickUpLocation { get; set; } = string.Empty;

    [Required] public double PickUpLat { get; set; }
    [Required] public double PickUpLng { get; set; }
}
using Api.Domain.Models;

namespace Api.Scheduling;

public class StudentClusterInfo
{
    public Student StudentInfo { get; set; } = null!;
    
    public string PickUpLocation { get; set; } = string.Empty;
    public double PickUpLat { get; set; }
    public double PickUpLng { get; set; }
    public int ClusterId { get; set; }
}
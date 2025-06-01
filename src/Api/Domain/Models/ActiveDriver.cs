namespace Api.Domain.Models;

public class ActiveDriver: BaseModel
{
    public Guid Id { get; set; }
    
    public Guid DriverId { get; set; }
    
    public Guid SchoolId { get; set; }
    
    public int SeatingCapacity { get; set; }
    
    public int Used { get; set; }
    public double TotalDistanceKm { get; set; }
    public DateTime? ExpiredAt { get; set; }
    
    public Driver Driver { get; set; } = null!;
    
    public School School { get; set; } = null!;
} 
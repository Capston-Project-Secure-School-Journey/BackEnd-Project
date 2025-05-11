namespace Api.DTOs.Scheduling;

public class DriverData
{
    public Guid Id { get; set; }
    public int SeatingCapacity { get; set; }
    public int Used { get; set; }
}
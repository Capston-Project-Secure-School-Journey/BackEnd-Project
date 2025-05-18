using Api.DTOs.Scheduling;

namespace Api.Scheduling;

public class DriverClusterInfo
{
    public DriverData  DriverData { get; set; } = null!;
    public int ClusterId { get; set; }
    public Centroid Centroid  { get; set; } = null!;
    public List<StudentClusterInfo> Students { get; set; } = null!;
}

public class Centroid
{
    public double X { get; set; }
    public double Y { get; set; }
}
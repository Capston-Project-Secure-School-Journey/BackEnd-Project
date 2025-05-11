using Api.Domain.Models;
using Api.DTOs.Scheduling;

namespace Api.Scheduling;

public interface IStudentGroupingAlgorithm
{
    Dictionary<DriverData, List<Student>> AllocateStudentsToBuses(List<Student> students,
        ref List<DriverData> driveList);

    static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371e3;
        var latRad1 = lat1 * Math.PI / 180;
        var latRad2 = lat2 * Math.PI / 180;
        var deltaLat = (lat2 - lat1) * Math.PI / 180;
        var deltaLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(latRad1) * Math.Cos(latRad2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));


        return r * c;
    }
}
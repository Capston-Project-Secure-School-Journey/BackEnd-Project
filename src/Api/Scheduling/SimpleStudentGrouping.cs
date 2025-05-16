using Api.Domain.Models;
using Api.DTOs.Scheduling;

namespace Api.Scheduling;

public class SimpleStudentGrouping : IStudentGroupingAlgorithm
{
    public Dictionary<DriverData, List<Student>> AllocateStudentsToBuses(List<Student> students,
        ref List<DriverData> driveList)
    {
        if (students.Distinct().Count() != students.Count)
            throw new InvalidDataException("Students must be unique");

        if (driveList.Sum(x => x.SeatingCapacity) < students.Distinct().Count())
            throw new InvalidDataException("There are no students to assign");

        driveList = driveList
            .OrderBy(x => x.Used)
            .ThenByDescending(x => x.SeatingCapacity)
            .ToList();

        var remaining = new List<Student>(students);
        var assignments = new Dictionary<DriverData, List<Student>>();
        var flag = 0;

        for (var busId = 0; busId <= driveList.Count - 1; busId++)
        {
            if (remaining.Count == 0)
                break;
            
            var (startLat, startLng) = FindStartingPoint(students, flag);
            var distances = remaining.Select(s => new
            {
                Student = s,
                Distance = IStudentGroupingAlgorithm.Haversine(startLat, startLng, s.PickUpLat, s.PickUpLng)
            }).OrderBy(x => x.Distance).Take(driveList[busId].SeatingCapacity).ToList();

            assignments[driveList[busId]] = distances.Select(x => x.Student).ToList();
            var assignedIds = distances.Select(x => x.Student.Id).ToList();
            remaining = remaining.Where(s => !assignedIds.Contains(s.Id)).ToList();

            driveList[busId].Used += distances.Count;
            flag = (flag + 1) % 4;
        }

        return assignments;
    }

    private static (double lat, double lng) FindStartingPoint(List<Student> students, int flag)
    {
        double lat = 0, lng = 0;
        switch (flag)
        {
            case 0:
                lat = students.Min(s => s.PickUpLat);
                lng = students.Min(s => s.PickUpLng);
                break;
            case 1:
                lat = students.Max(s => s.PickUpLat);
                lng = students.Min(s => s.PickUpLng);
                break;
            case 2:
                lat = students.Max(s => s.PickUpLat);
                lng = students.Max(s => s.PickUpLng);
                break;
            case 3:
                lat = students.Min(s => s.PickUpLat);
                lng = students.Max(s => s.PickUpLng);
                break;
        }

        return (lat, lng);
    }
}
using Accord.MachineLearning;
using Api.Domain.Models;
using Api.DTOs.Scheduling;

namespace Api.Scheduling;

public class KMeansStudentGrouping : IStudentGroupingAlgorithm
{
    public Dictionary<DriverData, List<Student>> AllocateStudentsToBuses(List<Student> students,
        ref List<DriverData> driveList)
    {
        var studentCount = students.Distinct().Count();
        if (studentCount != students.Count)
            throw new InvalidDataException("Students must be unique");

        var targetDrivers = GetTargetDrivers(driveList, studentCount);
        var clusteringInitiation = Clustering(targetDrivers, students);
        var driverClusterInfos = new List<DriverClusterInfo>();
        var i = 0;

        foreach (var driver in targetDrivers)
        {
            var studentsInCluster = clusteringInitiation
                .Where(x => x.ClusterId == i)
                .ToList();
            driverClusterInfos.Add(new DriverClusterInfo()
            {
                DriverData = driver,
                ClusterId = i,
                Centroid = CalculateCentroid(studentsInCluster),
                Students = studentsInCluster
            });
            i++;
        }

        do
        {
            var nextCluster = driverClusterInfos.FirstOrDefault(driverClusterInfo =>
                driverClusterInfo.Students.Count > driverClusterInfo.DriverData.SeatingCapacity);
            if (nextCluster == null)
                break;
            driverClusterInfos.Remove(nextCluster);
            
            var numberOfStudentNeedChangeCluster = nextCluster.Students.Count - nextCluster.DriverData.SeatingCapacity;
            var closestStudentDistances = nextCluster.Students
                .Select(student =>
                {
                    var closestCluster = driverClusterInfos
                        .Select(cluster => new
                        {
                            Cluster = cluster,
                            Distance = IStudentGroupingAlgorithm.Haversine(
                                cluster.Centroid.X,
                                cluster.Centroid.Y,
                                student.PickUpLat,
                                student.PickUpLng)
                        })
                        .OrderBy(x => x.Distance)
                        .First();

                    return new
                    {
                        Student = student,
                        ClosestCluster = closestCluster.Cluster,
                        closestCluster.Distance
                    };
                })
                .OrderBy(x => x.Distance)
                .Take(numberOfStudentNeedChangeCluster)
                .ToList();
            
            foreach (var distance in closestStudentDistances)
            {
                var student = clusteringInitiation.First(st => st.StudentInfo.Id == distance.Student.StudentInfo.Id);
                student.ClusterId = distance.ClosestCluster.ClusterId;
            }

            foreach (var driverClusterInfo in driverClusterInfos)
            {
                var studentsInCluster = clusteringInitiation
                    .Where(x => x.ClusterId == driverClusterInfo.ClusterId)
                    .ToList();
                driverClusterInfo.Centroid = CalculateCentroid(studentsInCluster);
                driverClusterInfo.Students = studentsInCluster;
            }
        } while (true);

        var assignments = new Dictionary<DriverData, List<Student>>();
        i = 0;
        foreach (var driver in targetDrivers)
        {
            var studentsInCluster = clusteringInitiation
                .Where(x => x.ClusterId == i)
                .ToList();
            assignments.Add(driver, studentsInCluster.Select(st => st.StudentInfo).ToList());
            // update driver.used, there are 2 trips (pick up and drop off)
            driver.Used +=  studentsInCluster.Count * 2;
            i++;
        }

        ResultExporter.Export(assignments);
        return assignments;
    }

    private static List<DriverData> GetTargetDrivers(List<DriverData> driveList, int studentCount)
    {
        if (driveList.Sum(x => x.SeatingCapacity) < studentCount)
            throw new InvalidDataException("Not enough drive capacity");

        var targetDrivers = new List<DriverData>();

        foreach (var driver in driveList
                     .OrderBy(x => x.Used))
        {
            if (0 == studentCount)
                break;
            targetDrivers.Add(driver);
            studentCount = driver.SeatingCapacity >= studentCount ? 0 : studentCount - driver.SeatingCapacity;
        }

        return targetDrivers;
    }

    private static List<StudentClusterInfo> Clustering(List<DriverData> targetDrivers, List<Student> students)
    {
        var studentClusterInfos = students.Select(st => new StudentClusterInfo()
            {
                StudentInfo = st,
                PickUpLat = st.PickUpLat,
                PickUpLng = st.PickUpLng,
                ClusterId = -1
            })
            .ToList();
        var points = students
            .Select(s => new[] { s.PickUpLat, s.PickUpLng })
            .ToArray();

        var kMeans = new KMeans(k: targetDrivers.Count);
        kMeans.MaxIterations = 1000;
        kMeans.UseSeeding = Seeding.KMeansPlusPlus;
        var clusters = kMeans.Learn(points);

        var labels = clusters.Decide(points);
        for (var i = 0; i < studentClusterInfos.Count; i++)
        {
            studentClusterInfos[i].ClusterId = labels[i];
        }

        return studentClusterInfos;
    }

    private static Centroid CalculateCentroid(double[][] points)
    {
        double sumX = 0;
        double sumY = 0;

        foreach (var point in points)
        {
            sumX += point[0];
            sumY += point[1];
        }

        return new Centroid() { X = sumX / points.Length, Y = sumY / points.Length };
    }

    private static Centroid CalculateCentroid(List<StudentClusterInfo> studentClusterInfos)
    {
        var points = studentClusterInfos
            .Select(s => new[] { s.PickUpLat, s.PickUpLng })
            .ToArray();

        return CalculateCentroid(points);
    }
}
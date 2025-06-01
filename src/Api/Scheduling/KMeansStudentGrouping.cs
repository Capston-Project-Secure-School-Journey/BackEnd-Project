using Accord.MachineLearning;
using Api.Domain.Models;
using Api.DTOs.Scheduling;

namespace Api.Scheduling;

public class KMeansStudentGrouping : IStudentGroupingAlgorithm
{
    public Dictionary<DriverData, List<Student>> AllocateStudentsToBuses(List<Student> students,
        ref List<DriverData> driveList)
    {
        var studentCount = students.DistinctBy(st => st.Id).Count();
        if (studentCount != students.Count)
            throw new InvalidDataException("Students must be unique");

        var targetDrivers = GetTargetDrivers(driveList, studentCount);
        var clusteringInitiation = Clustering(targetDrivers, students);


        ResolveClusterOverCapacity(BuildDriverClusterInfos(targetDrivers, clusteringInitiation),
            ref clusteringInitiation);

        OptimizeDriverLoadBalance(BuildDriverClusterInfos(targetDrivers, clusteringInitiation),
            ref clusteringInitiation);

        var assignments = new Dictionary<DriverData, List<Student>>();
        var i = 0;
        foreach (var driver in targetDrivers)
        {
            var studentsInCluster = clusteringInitiation
                .Where(x => x.ClusterId == i)
                .ToList();
            assignments.Add(driver, studentsInCluster.Select(st => st.StudentInfo).ToList());
            i++;
        }

        ResultExporter.Export(assignments);
        return assignments;
    }

    private static void ResolveClusterOverCapacity(List<DriverClusterInfo> driverClusterInfos,
        ref List<StudentClusterInfo> clusteringInitiation)
    {
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
    }

    private static void OptimizeDriverLoadBalance(List<DriverClusterInfo> driverClusterInfos,
        ref List<StudentClusterInfo> clusteringInitiation)
    {
        const int maxIterations = 100000;
        var movedStudentIds = new HashSet<Guid>();
        int iterations = 0;

        while (!AreAllDriversUtilizationAboveThreshold(driverClusterInfos) &&
               iterations++ < maxIterations)
        {
            var underUtilizedDriver = driverClusterInfos
                .OrderBy(d => (float)d.Students.Count / d.DriverData.SeatingCapacity)
                .FirstOrDefault();

            var candidate = clusteringInitiation
                .Where(s => s.ClusterId != underUtilizedDriver!.ClusterId &&
                            !movedStudentIds.Contains(s.StudentInfo.Id))
                .Select(s =>
                {
                    var distance = IStudentGroupingAlgorithm.Haversine(
                        underUtilizedDriver!.Centroid.X,
                        underUtilizedDriver.Centroid.Y,
                        s.StudentInfo.PickUpLat,
                        s.StudentInfo.PickUpLng);
                    return new { Student = s, Distance = distance };
                })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (candidate == null)
                break;
            
            candidate.Student.ClusterId = underUtilizedDriver!.ClusterId;
            movedStudentIds.Add(candidate.Student.StudentInfo.Id);

            foreach (var cluster in driverClusterInfos)
            {
                var studentsInCluster = clusteringInitiation
                    .Where(x => x.ClusterId == cluster.ClusterId)
                    .ToList();
                cluster.Students = studentsInCluster;
                cluster.Centroid = CalculateCentroid(studentsInCluster);
            }
        }
    }

    private static List<DriverClusterInfo> BuildDriverClusterInfos(List<DriverData> targetDrivers,
        List<StudentClusterInfo> clusteringInitiation)
    {
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

        return driverClusterInfos;
    }

    private static bool AreAllDriversUtilizationAboveThreshold(List<DriverClusterInfo> driverClusterInfos,
        float threshold = 0.5f)
    {
        return driverClusterInfos.All(driver =>
            (float)driver.Students.Count / driver.DriverData.SeatingCapacity > threshold);
    }


    private static List<DriverData> GetTargetDrivers(List<DriverData> driveList, int studentCount)
    {
        if (driveList.Sum(x => x.SeatingCapacity) < studentCount)
            throw new InvalidDataException("Not enough drive capacity");

        if (driveList.Any(x => x.SeatingCapacity == 0))
            throw new InvalidDataException("Seat capacity is zero");

        var targetDrivers = new List<DriverData>();
        var count = studentCount;

        foreach (var driver in driveList
                     .OrderBy(x => x.Used))
        {
            targetDrivers.Add(driver);
            studentCount = driver.SeatingCapacity >= studentCount ? 0 : studentCount - driver.SeatingCapacity;

            // ReSharper disable once PossibleLossOfFraction
            if ((float)count / targetDrivers.Sum(x => x.SeatingCapacity) < 0.7f)
                break;

            if (targetDrivers.Count == driveList.Count)
                break;
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
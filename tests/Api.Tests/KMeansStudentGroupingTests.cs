using Api.Scheduling;
using Api.Domain.Models;
using Api.DTOs.Scheduling;

namespace Api.Tests;

public class KMeansStudentGroupingTests
{
    private static KMeansStudentGrouping CreateSut()
    {
        return new KMeansStudentGrouping();
    }

    [Fact]
    public void AllocateStudentsToBuses_Throws_WhenStudentsNotUnique()
    {
        // Arrange
        var sut = CreateSut();
        var student = new Student { Id = Guid.NewGuid(), PickUpLat = 10, PickUpLng = 20 };
        var students = new List<Student> { student, student };
        var drivers = new List<DriverData> { new DriverData { Id = Guid.NewGuid(), SeatingCapacity = 2 } };

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => sut.AllocateStudentsToBuses(students, ref drivers));
    }

    [Fact]
    public void AllocateStudentsToBuses_Throws_WhenNotEnoughCapacity()
    {
        // Arrange
        var sut = CreateSut();
        var students = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), PickUpLat = 10, PickUpLng = 20 },
            new Student { Id = Guid.NewGuid(), PickUpLat = 11, PickUpLng = 21 }
        };
        var drivers = new List<DriverData> { new DriverData { Id = Guid.NewGuid(), SeatingCapacity = 1 } };

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => sut.AllocateStudentsToBuses(students, ref drivers));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void AllocateStudentsToBuses_AssignsStudentsToDrivers(int studentCount)
    {
        // Arrange
        var random = new Random();
        var minLat = 10.75;
        var maxLat = 10.85;
        var minLng = 106.65;
        var maxLng = 106.75;
        var sut = CreateSut();
        var students = new List<Student>();
        var drivers = new List<DriverData>();

        for (var i = 0; i < studentCount; i++)
        {
            var lat = minLat + (random.NextDouble() * (maxLat - minLat));
            var lng = minLng + (random.NextDouble() * (maxLng - minLng));
            students.Add(new Student()
            {
                Id = Guid.NewGuid(),
                PickUpLat = lat,
                PickUpLng = lng
            });
        }

        var capacity = 0;
        do
        {
            if (studentCount <= capacity)
                break;
            var driver = new DriverData { Id = Guid.NewGuid(), SeatingCapacity = random.Next(16, 45) };
            drivers.Add(driver);
            capacity += driver.SeatingCapacity;
        } while (true);

        // Act
        var result = sut.AllocateStudentsToBuses(students, ref drivers);

        // Assert
        Assert.Equal(students.Count, result.Sum(g => g.Value.Count));
        foreach (var group in result)
        {
            Assert.True(group.Key.SeatingCapacity >= group.Value.Count);
        }
        var allAssignedStudents = result.SelectMany(g => g.Value).ToList();
        var duplicateStudentIds = allAssignedStudents
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .ToList();
        Assert.Empty(duplicateStudentIds);
    }
    
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(5, 2)]
    [InlineData(10, 2)]
    [InlineData(50, 4)]
    [InlineData(100, 7)]
    [InlineData(500, 50)]
    [InlineData(1000, 100)]
    public void AllocateStudentsToBuses_DistributionShouldBeFair(int studentCount, int driverCount)
    {
        // Arrange
        var sut = CreateSut();
        var random = new Random();
        var minLat = 10.75;
        var maxLat = 10.85;
        var minLng = 106.65;
        var maxLng = 106.75;
        var students = new List<Student>();
        
        for (var i = 0; i < studentCount; i++)
        {
            var lat = minLat + (random.NextDouble() * (maxLat - minLat));
            var lng = minLng + (random.NextDouble() * (maxLng - minLng));
            students.Add(new Student()
            {
                Id = Guid.NewGuid(),
                PickUpLat = lat,
                PickUpLng = lng
            });
        }
        var drivers = Enumerable.Range(0, driverCount)
            .Select(_ => new DriverData { Id = Guid.NewGuid(), SeatingCapacity = 15 })
            .ToList();

        // Act
        var result = sut.AllocateStudentsToBuses(students, ref drivers);

        var assignedCounts = result.Values.Select(list => list.Count).ToList();
        var avg = assignedCounts.Average();
        var stdDev = Math.Sqrt(assignedCounts.Average(x => Math.Pow(x - avg, 2)));

        // Assert
        Assert.True(stdDev < 3);
    }
}
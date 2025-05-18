using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.PickupScheduleService;
using Api.Scheduling;
using Api.Services.SchoolManagement;
using Api.Services.UploadFileService;
using Api.Services.UserService;
using Api.TransferDTOs.Responses;
using AutoMapper;
using MongoDB.Driver;

namespace Api.Services.PickupScheduleService;

public class PickupScheduleService(
    Context context,
    IUserService userService,
    IFileUploadService fileUploadService,
    IMapper mapper,
    ISchoolManagement schoolManagement,
    GoogleMapsService googleMapsService) : IPickupScheduleService
{
    public async Task<PickupSchedule> AddPickupSchedule(CreatePickupScheduleDto request)
    {
        var pickupSchedule = await CreatePickupScheduleFromDto(request);
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>(Context.MongoCollectionName);
        await DeletePickupSchedule(request);
        await pickupScheduleCollection.InsertOneAsync(pickupSchedule);

        return pickupSchedule;
    }

    public async Task<List<PickupSchedule>> AddPickupSchedule(List<CreatePickupScheduleDto> requests)
    {
        var pickupSchedules = new List<PickupSchedule>();
        var tasks = new List<Task>();
        foreach (var request in requests)
        {
            tasks.Add(DeletePickupSchedule(request));
            var pickupSchedule = await CreatePickupScheduleFromDto(request);
            pickupSchedules.Add(pickupSchedule);
        }

        await Task.WhenAll(tasks);
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>(Context.MongoCollectionName);
        await pickupScheduleCollection.InsertManyAsync(pickupSchedules);

        return pickupSchedules;
    }

    public async Task<PickupScheduleView> GetPickupScheduleView(DateOnly date, Guid schoolId)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>(Context.MongoCollectionName);
        var pickupSchedules = await pickupScheduleCollection
            .Aggregate()
            .Group(
                key => new
                {
                    key.Date,
                    key.SchoolId,
                    key.SessionType
                },
                g => new PickupScheduleGroupResult
                {
                    Id = new GroupKey
                    {
                        Date = g.Key.Date,
                        SchoolId = g.Key.SchoolId,
                        SessionType = g.Key.SessionType
                    },
                    TotalStudents = g.Sum(x => x.NumberOfStudents),
                    TotalTrips = g.Count(),
                }
            )
            .Sort(Builders<PickupScheduleGroupResult>.Sort
                .Ascending(x => x.Id.Date)
                .Descending(x => x.Id.SessionType)
            )
            .ToListAsync();

        var response = new PickupScheduleView();
        foreach (var sp in pickupSchedules)
        {
            if (response.Data.TryGetValue(sp.Id.Date, out _))
            {
                response.Data[sp.Id.Date].Add(new()
                {
                    Date = sp.Id.Date,
                    SchoolId = sp.Id.SchoolId,
                    SessionType = sp.Id.SessionType,
                    NumberOfStudents = sp.TotalStudents,
                    NumberOfTrips = sp.TotalTrips
                });
            }
            else
            {
                response.Data.Add(sp.Id.Date, [
                    new()
                    {
                        Date = sp.Id.Date,
                        SchoolId = sp.Id.SchoolId,
                        SessionType = sp.Id.SessionType,
                        NumberOfStudents = sp.TotalStudents,
                        NumberOfTrips = sp.TotalTrips
                    }
                ]);
            }
        }

        return response;
    }

    public async Task<List<PickupScheduleResponse>> GetPickupScheduleByDate(DateOnly date, Guid schoolId)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>(Context.MongoCollectionName);
        var projection = Builders<PickupSchedule>.Projection
            .Exclude(x => x.BestRoute)
            .Exclude(x => x.Students);

        var pickupSchedule = (await pickupScheduleCollection
                .Find(sp => sp.Date == date && sp.SchoolId == schoolId)
                .SortByDescending(x => x.SessionType)
                .ThenBy(x => x.Type)
                .ThenByDescending(x => x.NumberOfStudents)
                .Project<PickupSchedule>(projection)
                .ToListAsync())
            .Select(mapper.Map<PickupScheduleResponse>)
            .ToList();

        return pickupSchedule;
    }

    public async Task<PickupSchedule> GetPickupSchedule(Guid pickupScheduleId)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");

        var pickupSchedule = await pickupScheduleCollection
            .Find(sp => sp.Id == pickupScheduleId)
            .FirstOrDefaultAsync<PickupSchedule>();

        if (pickupSchedule == null)
            throw new NotFoundException("Pickup schedule not found");

        return pickupSchedule;
    }

    public async Task IsOwnerOfPickupSchedule(Guid pickupScheduleId, Guid schoolId)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");

        var exist = await pickupScheduleCollection
            .Find(sp => sp.Id == pickupScheduleId && sp.SchoolId == schoolId)
            .AnyAsync<PickupSchedule>();

        if (!exist)
            throw new NotFoundException("Pickup schedule not found");
    }

    private async Task<PickupSchedule> CreatePickupScheduleFromDto(CreatePickupScheduleDto pickupScheduleDto)
    {
        var driver = await userService.GetUser(pickupScheduleDto.DriverId, UserType.Driver) as Driver;
        var school = await schoolManagement.GetSchool(pickupScheduleDto.SchoolId);

        if (driver == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var pickupSchedule = new PickupSchedule()
        {
            DriverId = pickupScheduleDto.DriverId,
            SchoolId = pickupScheduleDto.SchoolId,
            SessionType = pickupScheduleDto.SessionType,
            SchoolName = school.SchoolName,
            Date = pickupScheduleDto.Date,
            DriverName = driver.FirstName + " " + driver.LastName,
            DriverAvatar = driver.AvatarKey == null
                ? ""
                : await fileUploadService.GeneratePreSignedDownloadUrlAsync(driver.AvatarKey.Value, 99999),
            VehicleType = driver.VehicleType,
            DriverGender = driver.Gender,
            LicenseNumber = driver.LicenseNumber,
            IsAllNotesRead = true,
            Type = pickupScheduleDto.Type,
            StartTime = GetStartTime(pickupScheduleDto.Type, pickupScheduleDto.SessionType, school),
            EndTime = null,
            NumberOfStudents = pickupScheduleDto.Students.Count,
            NumberOfCurrentStudents = pickupScheduleDto.Students.Count,
            JourneyStatus = JourneyStatus.NotStarted,
            Students = new()
        };


        foreach (var student in pickupScheduleDto.Students)
        {
            var studentOnBus = new StudentOnBus()
            {
                StudentId = student.Id,
                Parents = student.ManagedBy,
                PickupAddress = student.PickUpLocation,
                PickupLat = student.PickUpLat,
                PickupLng = student.PickUpLng,
                Gender = student.Gender,
                AvatarUrl = student.AvatarKey == null
                    ? ""
                    : await fileUploadService.GeneratePreSignedDownloadUrlAsync(student.AvatarKey.Value, 99999),
                ClassName = student.Class.ClassName,
                ClassId = student.ClassId,
                FullName = student.FullName,
                IsPickedUp = false,
                PickedUpTime = null,
                IsDroppedOff = false,
                DroppedOffTime = null,
                SkipPickup = false
            };
            pickupSchedule.Students.Add(studentOnBus);
        }

        await GetBestRoute(pickupSchedule, school);
        return pickupSchedule;
    }

    private async Task DeletePickupSchedule(CreatePickupScheduleDto pickupScheduleDto)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");
        await pickupScheduleCollection.DeleteOneAsync(pk =>
            pk.SchoolId == pickupScheduleDto.SchoolId
            && pk.Date == pickupScheduleDto.Date
            && pk.SessionType == pickupScheduleDto.SessionType);
    }

    private static TimeSpan GetStartTime(PickupScheduleType pickupScheduleType, SessionType sessionType, School school)
    {
        switch (pickupScheduleType)
        {
            case PickupScheduleType.PickUp:
                if (sessionType == SessionType.Morning)
                    return school.MorningStartTime - new TimeSpan(1, 0, 0);
                return school.AfternoonStartTime - new TimeSpan(1, 0, 0);
            case PickupScheduleType.DropOff:
                if (sessionType == SessionType.Afternoon)
                    return school.AfternoonEndTime;
                return school.MorningEndTime;
            default:
                throw new InvalidDataException("Invalid pickup schedule type");
        }
    }

    private async Task GetBestRoute(PickupSchedule pickupSchedule, School school)
    {
        string origin;
        string destination;
        var firstStudent = pickupSchedule
            .Students
            .OrderByDescending(x =>
                IStudentGroupingAlgorithm.Haversine(x.PickupLat, x.PickupLng, school.AddressLat,
                    school.AddressLng))
            .FirstOrDefault();
        if (firstStudent == null)
            throw new InvalidDataException("No first student found");

        var waypoints = pickupSchedule
            .Students
            .Where(x => x.StudentId != firstStudent.StudentId)
            .Select(x => x.PickupLat + "," + x.PickupLng)
            .ToList();

        if (pickupSchedule.Type == PickupScheduleType.PickUp)
        {
            origin = firstStudent.PickupLat + "," + firstStudent.PickupLng;
            destination = school.AddressLat + "," + school.AddressLng;
        }
        else
        {
            origin = school.AddressLat + "," + school.AddressLng;
            destination = firstStudent.PickupLat + "," + firstStudent.PickupLng;
        }

        pickupSchedule.BestRoute = await googleMapsService.GetOptimizedRouteAsync(origin, destination, waypoints);
    }
}
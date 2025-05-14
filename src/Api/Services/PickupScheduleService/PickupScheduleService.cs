using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.PickupScheduleService;
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
    IMapper mapper) : IPickupScheduleService
{
    public async Task<PickupSchedule> AddPickupSchedule(CreatePickupScheduleDto request)
    {
        var pickupSchedule = await CreatePickupScheduleFromDto(request);
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");
        await pickupScheduleCollection.InsertOneAsync(pickupSchedule);

        return pickupSchedule;
    }

    public async Task<List<PickupSchedule>> AddPickupSchedule(List<CreatePickupScheduleDto> requests)
    {
        var pickupSchedules = new List<PickupSchedule>();
        foreach (var request in requests)
        {
            var pickupSchedule = await CreatePickupScheduleFromDto(request);
            pickupSchedules.Add(pickupSchedule);
        }

        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");
        await pickupScheduleCollection.InsertManyAsync(pickupSchedules);

        return pickupSchedules;
    }

    public async Task<PickupScheduleView> GetPickupScheduleView(DateOnly date, Guid schoolId)
    {
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");
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
        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");

        var projection = Builders<PickupSchedule>.Projection
            .Exclude(x => x.BestRoute)
            .Exclude(x => x.Students);

        var pickupSchedule = (await pickupScheduleCollection
            .Find(sp => sp.Date == date && sp.SchoolId == schoolId)
            .Sort(Builders<PickupSchedule>.Sort
                .Ascending(x => x.SessionType)
                .Descending(y => y.NumberOfStudents)
            )
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
        if (driver == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var pickupSchedule = new PickupSchedule()
        {
            DriverId = pickupScheduleDto.DriverId,
            SchoolId = pickupScheduleDto.SchoolId,
            SessionType = pickupScheduleDto.SessionType,
            Date = pickupScheduleDto.Date,
            DriverName = driver.FirstName + " " + driver.LastName,
            DriverAvatar = driver.AvatarKey == null
                ? ""
                : await fileUploadService.GeneratePreSignedDownloadUrlAsync(driver.AvatarKey.Value, 99999),
            VehicleType = driver.VehicleType,
            DriverGender = driver.Gender,
            LicenseNumber = driver.LicenseNumber,
            IsAllNotesRead = true,
            NumberOfStudents = pickupScheduleDto.Students.Count,
            NumberOfCurrentStudents = pickupScheduleDto.Students.Count,
            JourneyStatus = JourneyStatus.NotStarted,
            BestRoute = new(),
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
        
        return pickupSchedule;
    }
}

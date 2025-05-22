using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ShuttleScheduleService;
using Api.Extensions;
using Api.Scheduling;
using Api.Services.SchoolManagement;
using Api.Services.UploadFileService;
using Api.Services.UserService;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace Api.Services.ShuttleScheduleManagementService;

public class ShuttleScheduleManagementService(
    Context context,
    IUserService userService,
    IFileUploadService fileUploadService,
    IMapper mapper,
    ISchoolManagement schoolManagement,
    IMemoryCache cache) : IShuttleScheduleManagementService
{
    private const string CreateShuttleCacheKey = "CreateShuttleCacheKey";

    public async Task UpdateShuttleSchedule(ShuttleSchedule shuttleSchedule)
    {
        var filter = Builders<ShuttleSchedule>.Filter.Eq(s => s.Id, shuttleSchedule.Id);
        var update = Builders<ShuttleSchedule>.Update
            .Set(s => s.JourneyStatus, shuttleSchedule.JourneyStatus)
            .Set(s => s.EndJourneyTime, shuttleSchedule.EndJourneyTime)
            .Set(s => s.CancelReason, shuttleSchedule.CancelReason)
            .Set(s => s.NumberOfPickedUpStudents, shuttleSchedule.NumberOfPickedUpStudents)
            .Set(s => s.NumberOfDroppedOffStudents, shuttleSchedule.NumberOfDroppedOffStudents);

        await context.ShuttleScheduleCollection.UpdateOneAsync(filter, update);
    }

    public async Task UpdateStudentOnShuttleSchedule(Guid shuttleScheduleId, StudentOnBus studentOnBus)
    {
        var filter = Builders<ShuttleSchedule>.Filter.And(
            Builders<ShuttleSchedule>.Filter.Eq(t => t.Id, shuttleScheduleId),
            Builders<ShuttleSchedule>.Filter.ElemMatch(t => t.Students, s => s.StudentId == studentOnBus.StudentId)
        );

        var update = Builders<ShuttleSchedule>.Update
            .Set("Students.$.IsPickedUp", studentOnBus.IsPickedUp)
            .Set("Students.$.IsDroppedOff", studentOnBus.IsDroppedOff)
            .Set("Students.$.PickedUpTime", studentOnBus.PickedUpTime)
            .Set("Students.$.DroppedOffTime", studentOnBus.DroppedOffTime)
            .Set("Students.$.IsSkipUpReason", studentOnBus.IsSkipUpReason)
            .Set("Students.$.SkipPickup", studentOnBus.SkipPickup);

        await context.ShuttleScheduleCollection.UpdateOneAsync(filter, update);
    }

    public async Task<List<ShuttleSchedule>> AddShuttleSchedule(List<CreateShuttleScheduleDto> requests)
    {
        var shuttleSchedules = new List<ShuttleSchedule>();
        var tasks = new List<Task>();
        foreach (var request in requests)
        {
            tasks.Add(DeleteShuttleSchedule(request));
            var shuttleSchedule = await CreateShuttleScheduleFromDto(request);
            shuttleSchedules.Add(shuttleSchedule);
        }

        await Task.WhenAll(tasks);
        await context.ShuttleScheduleCollection.InsertManyAsync(shuttleSchedules);

        return shuttleSchedules;
    }

    public async Task<ShuttleScheduleView> GetShuttleScheduleView(DateOnly date, Guid schoolId)
    {
        var monthRange = DateTimeHelper.GetMonthRange(date);
        var shuttleSchedules = (await context.ShuttleScheduleCollection
                .Aggregate()
                .Group(
                    key => new
                    {
                        key.Date,
                        key.SchoolId,
                        key.SessionType,
                        key.Type
                    },
                    g => new ShuttleScheduleGroupResult
                    {
                        Id = new GroupKey
                        {
                            Date = g.Key.Date,
                            SchoolId = g.Key.SchoolId,
                            SessionType = g.Key.SessionType,
                            Type = g.Key.Type
                        },
                        TotalStudents = g.Sum(x => x.NumberOfStudents),
                        TotalTrips = g.Count(),
                    }
                )
                .Sort(Builders<ShuttleScheduleGroupResult>.Sort
                    .Descending(x => x.Id.Date)
                    .Descending(x => x.Id.SessionType)
                    .Ascending(x => x.Id.Type)
                )
                .ToListAsync())
            .Where(g => g.Id.SchoolId == schoolId
                        && g.Id.Date >= monthRange.StartOfMonth
                        && g.Id.Date <= monthRange.EndOfMonth);

        var response = new ShuttleScheduleView();
        foreach (var sp in shuttleSchedules)
        {
            if (response.Data.TryGetValue(sp.Id.Date, out _))
            {
                response.Data[sp.Id.Date].Add(new()
                {
                    Date = sp.Id.Date,
                    SchoolId = sp.Id.SchoolId,
                    Type = sp.Id.Type,
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
                        Type = sp.Id.Type,
                        SessionType = sp.Id.SessionType,
                        NumberOfStudents = sp.TotalStudents,
                        NumberOfTrips = sp.TotalTrips
                    }
                ]);
            }
        }

        return response;
    }

    public async Task<List<ShuttleScheduleResponse>> GetShuttleScheduleByDate(DateOnly date, Guid schoolId)
    {
        var projection = Builders<ShuttleSchedule>.Projection
            .Exclude(x => x.BestRoute)
            .Exclude(x => x.Students);

        var shuttleSchedule = (await context.ShuttleScheduleCollection
                .Find(sp => sp.Date == date && sp.SchoolId == schoolId)
                .SortByDescending(x => x.SessionType)
                .ThenBy(x => x.Type)
                .ThenByDescending(x => x.NumberOfStudents)
                .Project<ShuttleSchedule>(projection)
                .ToListAsync())
            .Select(mapper.Map<ShuttleScheduleResponse>)
            .ToList();

        return shuttleSchedule;
    }

    public async Task<ShuttleSchedule> GetShuttleSchedule(Guid shuttleScheduleId)
    {
        var shuttleSchedule = await context.ShuttleScheduleCollection
            .Find(sp => sp.Id == shuttleScheduleId)
            .FirstOrDefaultAsync<ShuttleSchedule>();

        if (shuttleSchedule == null)
            throw new NotFoundException("Shuttle schedule not found");

        return shuttleSchedule;
    }

    public async Task IsOwnerOfShuttleSchedule(Guid shuttleScheduleId, Guid schoolId)
    {
        var exist = await context.ShuttleScheduleCollection
            .Find(sp => sp.Id == shuttleScheduleId && sp.SchoolId == schoolId)
            .AnyAsync<ShuttleSchedule>();

        if (!exist)
            throw new NotFoundException("Shuttle schedule not found");
    }

    private async Task<ShuttleSchedule> CreateShuttleScheduleFromDto(CreateShuttleScheduleDto shuttleScheduleDto)
    {
        var driver = await GetDriverInCache(shuttleScheduleDto.DriverId);
        var school = await GetSchoolInCache(shuttleScheduleDto.SchoolId);

        if (driver == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);
        if (school == null)
            throw new NotFoundException(ErrorMessages.SchoolNotFound);

        var shuttleSchedule = new ShuttleSchedule()
        {
            DriverId = shuttleScheduleDto.DriverId,
            SchoolId = shuttleScheduleDto.SchoolId,
            SessionType = shuttleScheduleDto.SessionType,
            SchoolName = school.SchoolName,
            Date = shuttleScheduleDto.Date,
            DriverName = driver.FirstName + " " + driver.LastName,
            DriverAvatar = driver.AvatarKey == null
                ? ""
                : await GetImageUrl(driver.AvatarKey.Value),
            VehicleType = driver.VehicleType,
            DriverGender = driver.Gender,
            LicenseNumber = driver.LicenseNumber,
            IsAllNotesRead = true,
            Type = shuttleScheduleDto.Type,
            PickupStartTime = GetPickupStartTime(shuttleScheduleDto.Type, shuttleScheduleDto.SessionType, school),
            PickupEndTime = GetPickupEndTime(shuttleScheduleDto.Type, shuttleScheduleDto.SessionType, school),
            EndJourneyTime = null,
            NumberOfStudents = shuttleScheduleDto.Students.Count,
            NumberOfPickedUpStudents = 0,
            NumberOfDroppedOffStudents = 0,
            JourneyStatus = JourneyStatus.NotStarted,
            Students = new()
        };


        foreach (var student in shuttleScheduleDto.Students)
        {
            var studentOnBus = new StudentOnBus()
            {
                StudentId = student.Id,
                Parents = student.ManagedBy.Select(x => new ParentInfo()
                {
                    ParentId = x.ParentId,
                    Relationship = x.RelationshipWithStudent,
                    FullName = GetParentName(x.ParentId),
                    PhoneNumber = GetParentPhoneNumber(x.ParentId)
                }).ToList(),
                PickupAddress = student.PickUpLocation,
                PickupLat = student.PickUpLat,
                PickupLng = student.PickUpLng,
                Gender = student.Gender,
                AvatarUrl = student.AvatarKey == null
                    ? ""
                    : await GetImageUrl(student.AvatarKey.Value),
                ClassName = student.Class.ClassName,
                ClassId = student.ClassId,
                FullName = student.FullName,
                IsPickedUp = false,
                PickedUpTime = null,
                IsDroppedOff = false,
                DroppedOffTime = null,
                SkipPickup = false
            };
            shuttleSchedule.Students.Add(studentOnBus);
        }

        GetBestRoute(shuttleSchedule, school);
        return shuttleSchedule;
    }

    private async Task DeleteShuttleSchedule(CreateShuttleScheduleDto shuttleScheduleDto)
    {
        var shuttleScheduleCollection = context.MongoDatabase.GetCollection<ShuttleSchedule>("pickup_schedules");
        await shuttleScheduleCollection.DeleteManyAsync(pk =>
            pk.SchoolId == shuttleScheduleDto.SchoolId
            && pk.Date == shuttleScheduleDto.Date
            && pk.SessionType == shuttleScheduleDto.SessionType);
    }

    private static TimeSpan GetPickupStartTime(ShuttleScheduleType shuttleScheduleType, SessionType sessionType,
        School school)
    {
        switch (shuttleScheduleType)
        {
            case ShuttleScheduleType.PickUp:
                return sessionType == SessionType.Morning
                    ? school.MorningStartTime - new TimeSpan(2, 0, 0)
                    : school.AfternoonStartTime - new TimeSpan(2, 0, 0);
            case ShuttleScheduleType.DropOff:
                return sessionType == SessionType.Afternoon
                    ? school.AfternoonEndTime - new TimeSpan(0, 30, 0)
                    : school.MorningEndTime - new TimeSpan(0, 30, 0);
            default:
                throw new InvalidDataException("Invalid pickup schedule type");
        }
    }

    private static TimeSpan GetPickupEndTime(ShuttleScheduleType shuttleScheduleType, SessionType sessionType,
        School school)
    {
        switch (shuttleScheduleType)
        {
            case ShuttleScheduleType.PickUp:
                return sessionType == SessionType.Morning
                    ? school.MorningStartTime + new TimeSpan(0, 30, 0)
                    : school.AfternoonStartTime + new TimeSpan(0, 30, 0);
            case ShuttleScheduleType.DropOff:
                return sessionType == SessionType.Afternoon
                    ? school.AfternoonEndTime + new TimeSpan(0, 30, 0)
                    : school.MorningEndTime + new TimeSpan(0, 30, 0);
            default:
                throw new InvalidDataException("Invalid pickup schedule type");
        }
    }

    private void GetBestRoute(ShuttleSchedule shuttleSchedule, School school)
    {
        Point origin;
        Point destination;
        var nearestStudent = shuttleSchedule
            .Students
            .OrderByDescending(x =>
                IStudentGroupingAlgorithm.Haversine(x.PickupLat, x.PickupLng, school.AddressLat,
                    school.AddressLng))
            .FirstOrDefault();
        if (nearestStudent == null)
            throw new InvalidDataException("No first student found");

        var wayPoints = shuttleSchedule
            .Students
            .Where(x => x.StudentId != nearestStudent.StudentId)
            .Select(x => new Point()
            {
                FullAddress = x.PickupAddress,
                Latitude = x.PickupLat,
                Longitude = x.PickupLng,
            })
            .ToList();

        if (shuttleSchedule.Type == ShuttleScheduleType.PickUp)
        {
            origin = new Point()
            {
                FullAddress = nearestStudent.PickupAddress,
                Latitude = nearestStudent.PickupLat,
                Longitude = nearestStudent.PickupLng
            };
            destination = new Point()
            {
                FullAddress = school.Address,
                Latitude = school.AddressLat,
                Longitude = school.AddressLng
            };
        }
        else
        {
            origin = new Point()
            {
                FullAddress = school.Address,
                Latitude = school.AddressLat,
                Longitude = school.AddressLng
            };
            destination = new Point()
            {
                FullAddress = nearestStudent.PickupAddress,
                Latitude = nearestStudent.PickupLat,
                Longitude = nearestStudent.PickupLng
            };
        }

        shuttleSchedule.BestRoute = new BestRoute()
        {
            Origin = origin,
            Destination = destination,
            WayPoints = wayPoints
        };
    }

    private string GetParentName(Guid parentId)
    {
        var cacheKey = $"{parentId}_{CreateShuttleCacheKey}_Name";
        if (cache.TryGetValue(cacheKey, out string? parentName)) return parentName ?? string.Empty;
        parentName = context.Parents.Where(p => p.Id == parentId).Select(p => p.FirstName + " " + p.LastName)
            .FirstOrDefault() ?? string.Empty;
        cache.Set(cacheKey, parentName, TimeSpan.FromDays(1));
        return parentName;
    }

    private string GetParentPhoneNumber(Guid parentId)
    {
        var cacheKey = $"{parentId}_{CreateShuttleCacheKey}_Phone";
        if (cache.TryGetValue(cacheKey, out string? parentPhone)) return parentPhone ?? string.Empty;
        parentPhone = context.Parents.Where(p => p.Id == parentId).Select(p => p.PhoneNumber).FirstOrDefault() ??
                      string.Empty;
        cache.Set(cacheKey, parentPhone, TimeSpan.FromDays(1));
        return parentPhone;
    }

    private async Task<string> GetImageUrl(Guid fileManagementId)
    {
        var cacheKey = $"{fileManagementId}_{CreateShuttleCacheKey}_FileManagement";
        if (cache.TryGetValue(cacheKey, out string? url)) return url ?? string.Empty;
        url = await fileUploadService.GeneratePreSignedDownloadUrlAsync(fileManagementId, 99999);
        cache.Set(cacheKey, url, TimeSpan.FromDays(1));
        return url;
    }

    private async Task<Driver?> GetDriverInCache(Guid driverId)
    {
        var cacheKey = $"{driverId}_{CreateShuttleCacheKey}_Driver";
        if (cache.TryGetValue(cacheKey, out Driver? driver)) return driver ?? null;
        driver = await userService.GetUser(driverId, UserType.Driver) as Driver;
        cache.Set(cacheKey, driver, TimeSpan.FromDays(1));
        return driver;
    }

    private async Task<School?> GetSchoolInCache(Guid schoolId)
    {
        var cacheKey = $"{schoolId}_{CreateShuttleCacheKey}_School";
        if (cache.TryGetValue(cacheKey, out School? school)) return school ?? null;
        school = await schoolManagement.GetSchool(schoolId);
        cache.Set(cacheKey, school, TimeSpan.FromDays(1));
        return school;
    }
}
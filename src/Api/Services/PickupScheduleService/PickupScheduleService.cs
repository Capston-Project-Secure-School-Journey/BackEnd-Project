using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.PickupScheduleService;
using Api.Services.UploadFileService;
using Api.Services.UserService;

namespace Api.Services.PickupScheduleService;

public class PickupScheduleService(
    Context context,
    IUserService userService,
    IFileUploadService fileUploadService) : IPickupScheduleService
{
    public async Task<PickupSchedule> AddPickupSchedule(CreatePickupScheduleServiceDto request)
    {
        var driver = await userService.GetUser(request.DriverId, UserType.Driver) as Driver;
        if (driver == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var pickupSchedule = new PickupSchedule()
        {
            DriverId = request.DriverId,
            SchoolId = request.SchoolId,
            SessionType = request.SessionType,
            Date = request.Date,
            DriverName = driver.FirstName + " " + driver.LastName,
            DriverAvatar = driver.AvatarKey == null
                ? ""
                : await fileUploadService.GeneratePreSignedDownloadUrlAsync(driver.AvatarKey.Value, 99999),
            VehicleType = driver.VehicleType,
            DriverGender = driver.Gender,
            LicenseNumber = driver.LicenseNumber,
            IsAllNotesRead = true,
            NumberOfStudents = request.Students.Count,
            NumberOfCurrentStudents = request.Students.Count,
            JourneyStatus = JourneyStatus.NotStarted,
            BestRoute = new(),
            Students = new()
        };


        foreach (var student in request.Students)
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

        var pickupScheduleCollection = context.MongoDatabase.GetCollection<PickupSchedule>("pickup_schedules");
        await pickupScheduleCollection.InsertOneAsync(pickupSchedule);

        return pickupSchedule;
    }
}
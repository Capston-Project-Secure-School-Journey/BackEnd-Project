using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ApprovalProcessor;
using Api.Extensions;
using Api.Services.SchoolManagement;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ApprovalProcessor;

public class ApprovalProcessor(
    Context context,
    IFileUploadService fileUploadService,
    ISchoolManagement schoolManagement) : IApprovalProcessor
{
    public async Task<DriverApprovalRequest> CreateApplication(Guid driverId, Guid schoolId)
    {
        await schoolManagement.GetSchool(schoolId);
        var driver = await context
            .Drivers
            .FirstOrDefaultAsync(dr => dr.Id == driverId);
        ValidateDriverInfo(driver!);
        var isApplied = await context
            .DriverApprovalRequests
            .Where(x => x.DriverId == driverId)
            .Where(x => x.RequestStatus != RequestStatus.Rejected && x.RequestStatus != RequestStatus.Cancelled)
            .AnyAsync();

        if (isApplied)
            throw new BadRequestException(ErrorMessages.AlreadyApplied);

        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            var application = new DriverApprovalRequest
            {
                SchoolId = schoolId,
                RequestedDate = DateTimeHelper.GetDateTimeUtc7(),
                MotivationLetter = string.Empty,
                DriverId = driverId,
                RequestStatus = RequestStatus.Created,
                ApprovedBy = null,
                VehicleType = driver!.VehicleType,
                LicenseNumber = driver.LicenseNumber,
                SeatingCapacity = driver.SeatingCapacity,
                LastCheckDrivingLicense = driver.LastCheckDrivingLicense
            };

            await context.DriverApprovalRequests.AddAsync(application);
            await context.SaveChangesAsync();

            foreach (var i in driver.VehicleImages)
            {
                var response = await fileUploadService.CopyObjectAsync(i.FileManagementId,
                    $"applications/{driverId}/{application.Id}/vehicle_images");
                application.VehicleImages.Add(new FileMetadata()
                {
                    FileManagementId = response.Key,
                    Key = response.S3Key
                });
            }

            foreach (var i in driver.DriverInformationImages)
            {
                var response = await fileUploadService.CopyObjectAsync(i.FileManagementId,
                    $"applications/{driverId}/{application.Id}/driver_information_images");
                application.DriverInformationImages.Add(new DriverInformationImage()
                {
                    FileManagementId = response.Key,
                    Key = response.S3Key,
                    Type = i.Type
                });
            }

            var stateHistory = new DriverRequestStatusHistory()
            {
                RequestId = application.Id,
                FromStatus = null,
                ToStatus = RequestStatus.Created,
                ChangedBy = driverId,
                ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
                Note = string.Empty
            };
            context.DriverApprovalRequests.Update(application);
            await context.DriverRequestStatusHistories.AddAsync(stateHistory);

            await context.SaveChangesAsync();
            await trans.CommitAsync();
            return application;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<DriverApprovalRequest> UpdateApplication(Guid applicationId, Guid driverId)
    {
        var application = await GetApplicationByDriver(applicationId, driverId);
        var driver = await context
            .Drivers
            .FirstOrDefaultAsync(dr => dr.Id == application.DriverId);

        var actionsCanDo = GetActionCanDo(application, false);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Update))
            throw new BadRequestException(ErrorMessages.CannotUpdateApplication);

        ValidateDriverInfo(driver!);
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            application.VehicleType = driver!.VehicleType;
            application.LicenseNumber = driver.LicenseNumber;
            application.SeatingCapacity = driver.SeatingCapacity;
            application.LastCheckDrivingLicense = driver.LastCheckDrivingLicense;

            await fileUploadService.DeleteFileManagementAsync(application.VehicleImages.Select(x => x.FileManagementId)
                .ToList());
            await fileUploadService.DeleteFileManagementAsync(application.DriverInformationImages
                .Select(x => x.FileManagementId)
                .ToList());
            application.VehicleImages.Clear();
            application.DriverInformationImages.Clear();

            foreach (var i in driver.VehicleImages)
            {
                var response = await fileUploadService.CopyObjectAsync(i.FileManagementId,
                    $"applications/{driverId}/{application.Id}/vehicle_images");
                application.VehicleImages.Add(new FileMetadata()
                {
                    FileManagementId = response.Key,
                    Key = response.S3Key
                });
            }

            foreach (var i in driver.DriverInformationImages)
            {
                var response = await fileUploadService.CopyObjectAsync(i.FileManagementId,
                    $"applications/{driverId}/{application.Id}/driver_information_images");
                application.DriverInformationImages.Add(new DriverInformationImage()
                {
                    FileManagementId = response.Key,
                    Key = response.S3Key,
                    Type = i.Type
                });
            }

            if (application.RequestStatus != RequestStatus.Created)
            {
                var stateHistory = new DriverRequestStatusHistory()
                {
                    RequestId = application.Id,
                    FromStatus = application.RequestStatus,
                    ToStatus = RequestStatus.Pending,
                    ChangedBy = driverId,
                    ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
                    Note = string.Empty
                };
                if (application.RequestStatus != RequestStatus.Created)
                    application.RequestStatus = RequestStatus.Pending;
                await context.DriverRequestStatusHistories.AddAsync(stateHistory);
            }

            context.DriverApprovalRequests.Update(application);

            await context.SaveChangesAsync();
            await trans.CommitAsync();
            return application;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task SubmitApplication(Guid applicationId, Guid driverId)
    {
        var application = await GetApplicationByDriver(applicationId, driverId);
        var actionsCanDo = GetActionCanDo(application, false);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Submit))
            throw new BadRequestException(ErrorMessages.CannotSubmitApplication);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.Pending,
            ChangedBy = driverId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = string.Empty
        };

        application.RequestStatus = RequestStatus.Pending;
        context.DriverApprovalRequests.Update(application);
        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task ApproveApplication(Guid applicationId, Guid reviewerId)
    {
        var application = await GetApplicationByReviewer(applicationId, reviewerId);
        var actionsCanDo = GetActionCanDo(application, true);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Approve))
            throw new BadRequestException(ErrorMessages.CannotAcceptApplication);

        var driver = await context
            .Drivers
            .FirstOrDefaultAsync(dr => dr.Id == application.DriverId);

        if (driver == null)
            throw new BadRequestException(ErrorMessages.SystemError);

        driver.VerifiedBy.Add(new VerifiedBy()
            { SchoolId = application.SchoolId, VerifiedAt = DateTimeHelper.GetDateTimeUtc7() });
        context.Drivers.Update(driver);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.Approved,
            ChangedBy = reviewerId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = string.Empty
        };

        application.RequestStatus = RequestStatus.Approved;
        application.ApprovedBy = reviewerId;

        context.DriverApprovalRequests.Update(application);
        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task RejectApplication(Guid applicationId, Guid reviewerId, string reason)
    {
        var application = await GetApplicationByReviewer(applicationId, reviewerId);
        var actionsCanDo = GetActionCanDo(application, true);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Reject))
            throw new BadRequestException(ErrorMessages.CannotRejectApplication);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.Rejected,
            ChangedBy = reviewerId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = reason
        };

        application.RequestStatus = RequestStatus.Rejected;
        context.DriverApprovalRequests.Update(application);
        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task RequireAdditionalDetails(Guid applicationId, Guid reviewerId, string reason)
    {
        var application = await GetApplicationByReviewer(applicationId, reviewerId);
        var actionsCanDo = GetActionCanDo(application, true);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.RequestMoreInfo))
            throw new BadRequestException(ErrorMessages.SystemError);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.NeedMoreInfo,
            ChangedBy = reviewerId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = reason
        };

        application.RequestStatus = RequestStatus.NeedMoreInfo;
        context.DriverApprovalRequests.Update(application);

        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task CancelApplicationByReviewer(Guid applicationId, Guid reviewerId, string reason)
    {
        var application = await GetApplicationByReviewer(applicationId, reviewerId);
        var actionsCanDo = GetActionCanDo(application, true);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Cancel))
            throw new BadRequestException(ErrorMessages.CannotCancelApplication);

        var driver = await context
            .Drivers
            .FirstOrDefaultAsync(dr => dr.Id == application.DriverId);

        if (driver == null)
            throw new BadRequestException(ErrorMessages.SystemError);

        var count = driver.VerifiedBy.RemoveAll(x => x.SchoolId == application.SchoolId);
        if (count > 0)
            context.Drivers.Update(driver);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.Cancelled,
            ChangedBy = reviewerId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = reason
        };

        application.RequestStatus = RequestStatus.Cancelled;
        context.DriverApprovalRequests.Update(application);

        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task CancelApplicationByDriver(Guid applicationId, Guid driverId, string reason)
    {
        var application = await GetApplicationByDriver(applicationId, driverId);
        var actionsCanDo = GetActionCanDo(application, false);

        if (actionsCanDo.All(x => x.Action != ApplicationAction.Cancel))
            throw new BadRequestException(ErrorMessages.CannotCancelApplication);

        var stateHistory = new DriverRequestStatusHistory()
        {
            RequestId = application.Id,
            FromStatus = application.RequestStatus,
            ToStatus = RequestStatus.Cancelled,
            ChangedBy = driverId,
            ChangedAt = DateTimeHelper.GetDateTimeUtc7(),
            Note = reason
        };

        application.RequestStatus = RequestStatus.Cancelled;
        context.DriverApprovalRequests.Update(application);

        await context.DriverRequestStatusHistories.AddAsync(stateHistory);
        await context.SaveChangesAsync();
    }

    public async Task DeleteApplicationByDriver(Guid applicationId, Guid driverId)
    {
        var application = await GetApplicationByDriver(applicationId, driverId);

        var actionsCanDo = GetActionCanDo(application, false);
        if (actionsCanDo.All(x => x.Action != ApplicationAction.Delete))
            throw new BadRequestException(ErrorMessages.CannotDeleteApplication);

        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await fileUploadService.DeleteFileManagementAsync(application.VehicleImages.Select(x => x.FileManagementId)
                .ToList());
            await fileUploadService.DeleteFileManagementAsync(application.DriverInformationImages
                .Select(x => x.FileManagementId)
                .ToList());

            await context
                .Entry(application)
                .Collection<DriverRequestStatusHistory>(x => x.DriverRequestStatusHistories)
                .LoadAsync();

            context.DriverRequestStatusHistories.RemoveRange(application.DriverRequestStatusHistories);
            context.DriverApprovalRequests.Remove(application);
            await context.SaveChangesAsync();
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ApplicationActionDto>> GetActionCanDoByDriver(Guid applicationId, Guid driverId)
    {
        var application = await GetApplicationByDriver(applicationId, driverId);
        return GetActionCanDo(application, false);
    }

    public async Task<List<ApplicationActionDto>> GetActionCanDoByReviewer(Guid applicationId, Guid reviewerId)
    {
        var application = await GetApplicationByReviewer(applicationId, reviewerId);
        return GetActionCanDo(application, true);
    }

    private static List<ApplicationActionDto> GetActionCanDo(DriverApprovalRequest application, bool isReviewer)
    {
        var actions = new List<ApplicationActionDto>();
        switch (application.RequestStatus)
        {
            case RequestStatus.Approved:
                if (isReviewer) actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Cancel });
                break;
            case RequestStatus.Rejected:
                break;
            case RequestStatus.Cancelled:
                break;
            case RequestStatus.NeedMoreInfo:
                if (!isReviewer)
                {
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Update });
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Cancel });
                }

                break;
            case RequestStatus.Pending:
                if (isReviewer)
                {
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Approve });
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Reject });
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.RequestMoreInfo });
                }
                else
                {
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Cancel });
                }

                break;
            case RequestStatus.Created:
                if (!isReviewer)
                {
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Submit });
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Update });
                    actions.Add(new ApplicationActionDto() { Action = ApplicationAction.Delete });
                }

                break;
        }

        return actions;
    }

    private async Task<DriverApprovalRequest> GetApplicationByDriver(Guid applicationId, Guid driverId)
    {
        var application = await context.DriverApprovalRequests
            .FirstOrDefaultAsync(x => x.Id == applicationId && x.DriverId == driverId);

        if (application == null)
            throw new NotFoundException(ErrorMessages.ApplicationNotFound);

        return application;
    }

    private async Task<DriverApprovalRequest> GetApplicationByReviewer(Guid applicationId, Guid reviewerId)
    {
        var application = await context.DriverApprovalRequests
            .FirstOrDefaultAsync(x => x.Id == applicationId);

        if (application == null)
            throw new NotFoundException(ErrorMessages.ApplicationNotFound);

        if (await GetReviewerOfSchool(application.SchoolId) != reviewerId)
            throw new NotFoundException(ErrorMessages.ApplicationNotFound);
        return application;
    }

    public async Task<Guid> GetReviewerOfSchool(Guid schoolId)
    {
        var reviewer = await context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (reviewer == null)
            throw new BadRequestException(ErrorMessages.SystemError);

        return reviewer.Id;
    }

    private static void ValidateDriverInfo(Driver driver)
    {
        if (driver == null)
            throw new BadRequestException(ErrorMessages.SystemError);

        if (string.IsNullOrEmpty(driver.LicenseNumber))
            throw new BadRequestException(ErrorMessages.MissingLicenseNumber);
        if (string.IsNullOrEmpty(driver.VehicleType))
            throw new BadRequestException(ErrorMessages.MissingVehicleType);
        if (driver.SeatingCapacity <= 0)
            throw new BadRequestException(ErrorMessages.MissingSeatNumber);
        if (driver.DriverInformationImages.Count != 2)
            throw new BadRequestException(ErrorMessages.MissingLicenseImages);
        if (driver.VehicleImages.Count <= 4)
            throw new BadRequestException(ErrorMessages.RequireAtLeast5VehiclePhotos);
    }
}
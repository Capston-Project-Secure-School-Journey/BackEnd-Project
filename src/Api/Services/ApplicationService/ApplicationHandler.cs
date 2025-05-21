using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ApprovalProcessor;
using Api.Extensions;
using Api.Jobs;
using Api.Services.ApprovalProcessor;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ApplicationService;

public class ApplicationHandler(
    Context context,
    IApplicationService applicationService,
    IApprovalProcessor approvalProcessor,
    IMapper mapper,
    IFileUploadService fileUploadService)
    : IApplicationHandler
{
    public async Task<Pagination<ApplicationResponse>> GetApplicationsBySchool(Guid schoolId,
        GetDriverApprovalApplication request)
    {
        var queryable = await applicationService.GetApplicationsBySchool(schoolId, request.Status);
        var count = await queryable.CountAsync();
        var data = queryable
            .Pagination(request.Page, request.Limit)
            .AsEnumerable()
            .Select(mapper.Map<ApplicationResponse>)
            .ToList();
        var responses = new Pagination<ApplicationResponse>(data, request.Page, request.Limit, count);

        return responses;
    }

    public async Task<Pagination<ApplicationResponse>> GetApplicationsByDriver(Guid driverId,
        GetDriverApprovalApplication request)
    {
        var queryable = await applicationService.GetApplicationsByDriver(driverId, request.Status);
        var count = await queryable.CountAsync();
        var data = queryable.Pagination(request.Page, request.Limit)
            .AsEnumerable()
            .Select(mapper.Map<ApplicationResponse>)
            .ToList();

        var responses = new Pagination<ApplicationResponse>(data, request.Page, request.Limit, count);


        return responses;
    }

    public async Task<ApplicationResponse> GetApplication(Guid applicationId)
    {
        var application = await applicationService.GetApplication(applicationId);
        return await MapToResponse(application);
    }

    public async Task<ApplicationResponse> CreateApplication(Guid driverId, Guid schoolId)
    {
        var application = await approvalProcessor.CreateApplication(driverId, schoolId);
        return mapper.Map<ApplicationResponse>(application);
    }

    public async Task<ApplicationResponse> UpdateApplication(Guid applicationId, Guid driverId)
    {
        var application = await approvalProcessor.UpdateApplication(applicationId, driverId);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
        return mapper.Map<ApplicationResponse>(application);
    }

    public async Task SubmitApplication(Guid applicationId, Guid driverId)
    {
        await approvalProcessor.SubmitApplication(applicationId, driverId);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task ApproveApplication(Guid applicationId, Guid reviewerId)
    {
        await approvalProcessor.ApproveApplication(applicationId, reviewerId);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task RejectApplication(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.RejectApplication(applicationId, reviewerId, reason);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task RequireAdditionalDetails(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.RequireAdditionalDetails(applicationId, reviewerId, reason);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task RequestCancellationByReviewer(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.RequestCancellationByReviewer(applicationId, reviewerId, reason);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task RequestCancellationByDriver(Guid applicationId, Guid driverId, string reason)
    {
        await approvalProcessor.RequestCancellationByDriver(applicationId, driverId, reason);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task CancelApplication(Guid applicationId, Guid driverId)
    {
        await approvalProcessor.CancelApplication(applicationId, driverId);
        BackgroundJob.Enqueue<CreateApplicationNotificationJob>((job) => job.ExecuteAsync(applicationId));
    }

    public async Task DeleteApplicationByDriver(Guid applicationId, Guid driverId)
    {
        await approvalProcessor.DeleteApplicationByDriver(applicationId, driverId);
    }

    public async Task<List<ApplicationActionDto>> GetActionCanDoByReviewer(Guid applicationId, Guid reviewerId)
    {
        return await approvalProcessor.GetActionCanDoByReviewer(applicationId, reviewerId);
    }

    public async Task<List<ApplicationActionDto>> GetActionCanDoByDriver(Guid applicationId, Guid driverId)
    {
        return await approvalProcessor.GetActionCanDoByDriver(applicationId, driverId);
    }

    public async Task IsDriverOwnerOfApplication(Guid applicationId, Guid driverId)
    {
        var application = await context
            .DriverApprovalRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == applicationId && x.DriverId == driverId);

        if (application == null)
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    public async Task IsSchoolOwnerOfApplication(Guid applicationId, Guid schoolId)
    {
        var application = await context
            .DriverApprovalRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == applicationId && x.SchoolId == schoolId);

        if (application == null)
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    private async Task<ApplicationResponse> MapToResponse(DriverApprovalRequest request)
    {
        var entity = context.Entry(request);

        if (!entity.Collection<DriverRequestStatusHistory>(x => x.DriverRequestStatusHistories).IsLoaded)
            await entity.Collection<DriverRequestStatusHistory>(x => x.DriverRequestStatusHistories).LoadAsync();

        var response = mapper.Map<ApplicationResponse>(request);
        foreach (var file in request.VehicleImages)
        {
            var preSignDownload = await fileUploadService.GeneratePreSignedDownloadUrlAsync(file.Key);
            response.VehicleImages.Add(preSignDownload);
        }

        foreach (var file in request.DriverInformationImages)
        {
            var preSignDownload = await fileUploadService.GeneratePreSignedDownloadUrlAsync(file.Key);
            response.DriverInformationImages.Add(new DriverInformationImageUrl()
                { Url = preSignDownload, Type = file.Type });
        }

        foreach (var history in request.DriverRequestStatusHistories)
            response.DriverRequestStatusHistoryResponse.Add(mapper.Map<DriverRequestStatusHistoryResponse>(history));

        return response;
    }
}
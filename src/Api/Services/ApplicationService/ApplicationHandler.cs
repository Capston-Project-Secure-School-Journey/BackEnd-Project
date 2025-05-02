using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ApprovalProcessor;
using Api.Extensions;
using Api.Services.ApprovalProcessor;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Responses;
using AutoMapper;
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
    public async Task<List<ApplicationResponse>> GetApplicationsBySchool(Guid schoolId, int currentPage, int pageSize)
    {
        var applications = await applicationService.GetApplicationsBySchool(schoolId);
        var responses = applications.Pagination(currentPage, pageSize)
            .Select(mapper.Map<ApplicationResponse>)
            .ToList();

        return responses;
    }

    public async Task<List<ApplicationResponse>> GetApplicationsByDriver(Guid driverId, int currentPage, int pageSize)
    {
        var applications = await applicationService.GetApplicationsByDriver(driverId);
        var responses = applications.Pagination(currentPage, 5)
            .Select(mapper.Map<ApplicationResponse>)
            .ToList();

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
        return mapper.Map<ApplicationResponse>(application);
    }

    public async Task SubmitApplication(Guid applicationId, Guid driverId)
    {
        await approvalProcessor.SubmitApplication(applicationId, driverId);
    }

    public async Task ApproveApplication(Guid applicationId, Guid reviewerId)
    {
        await approvalProcessor.ApproveApplication(applicationId, reviewerId);
    }

    public async Task RejectApplication(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.RejectApplication(applicationId, reviewerId, reason);
    }

    public async Task RequireAdditionalDetails(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.RequireAdditionalDetails(applicationId, reviewerId, reason);
    }

    public async Task CancelApplicationByReviewer(Guid applicationId, Guid reviewerId, string reason)
    {
        await approvalProcessor.CancelApplicationByReviewer(applicationId, reviewerId, reason);
    }

    public async Task CancelApplicationByDriver(Guid applicationId, Guid driverId, string reason)
    {
        await approvalProcessor.CancelApplicationByDriver(applicationId, driverId, reason);
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
        {
            response.DriverRequestStatusHistoryResponse.Add(mapper.Map<DriverRequestStatusHistoryResponse>(history));
        }

        return response;
    }
}
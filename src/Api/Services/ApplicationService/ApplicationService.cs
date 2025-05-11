using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ApplicationService;

public class ApplicationService(
    Context context) : IApplicationService
{
    public Task<IQueryable<DriverApprovalRequest>> GetApplicationsBySchool(Guid schoolId, RequestStatus? status)
    {
        var applications = context
            .DriverApprovalRequests
            .AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.RequestStatus != RequestStatus.Created)
            .Where(x => status == null || x.RequestStatus == status.Value)
            .OrderBy(x => x.RequestedDate)
            .AsQueryable();

        return Task.FromResult(applications);
    }

    public Task<IQueryable<DriverApprovalRequest>> GetApplicationsByDriver(Guid driverId, RequestStatus? status)
    {
        var applications = context
            .DriverApprovalRequests
            .AsNoTracking()
            .Where(x => x.DriverId == driverId)
            .Where(x => status == null || x.RequestStatus == status.Value)
            .OrderBy(x => x.RequestedDate)
            .AsQueryable();

        return Task.FromResult(applications);
    }

    public async Task<DriverApprovalRequest> GetApplication(Guid applicationId)
    {
        var applications = await context
            .DriverApprovalRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == applicationId);

        if (applications == null)
            throw new NotFoundException(ErrorMessages.ApplicationNotFound);
        return applications;
    }

    public static string GetApplicationNotificationMessage(DriverApprovalRequest application)
    {
        return $"Đơn #{application.Id} có thay đổi.";
    }
}
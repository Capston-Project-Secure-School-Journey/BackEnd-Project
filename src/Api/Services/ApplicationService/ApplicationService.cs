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
    public Task<List<DriverApprovalRequest>> GetApplicationsBySchool(Guid schoolId)
    {
        var applications = context
            .DriverApprovalRequests
            .AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.RequestStatus != RequestStatus.Created)
            .OrderBy(x => x.RequestedDate)
            .ToList();

        return Task.FromResult(applications);
    }

    public Task<List<DriverApprovalRequest>> GetApplicationsByDriver(Guid driverId)
    {
        var applications = context
            .DriverApprovalRequests
            .AsNoTracking()
            .Where(x => x.DriverId == driverId)
            .OrderBy(x => x.RequestedDate)
            .ToList();

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
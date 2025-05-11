using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Services.ApplicationService;

public interface IApplicationService
{
    Task<IQueryable<DriverApprovalRequest>> GetApplicationsBySchool(Guid schoolId, RequestStatus? status);
    Task<IQueryable<DriverApprovalRequest>> GetApplicationsByDriver(Guid driverId, RequestStatus? status);
    Task<DriverApprovalRequest> GetApplication(Guid applicationId);
}
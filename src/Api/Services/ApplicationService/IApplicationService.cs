using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Services.ApplicationService;

public interface IApplicationService
{
    Task<List<DriverApprovalRequest>> GetApplicationsBySchool(Guid schoolId, RequestStatus? status);
    Task<List<DriverApprovalRequest>> GetApplicationsByDriver(Guid driverId, RequestStatus? status);
    Task<DriverApprovalRequest> GetApplication(Guid applicationId);
}
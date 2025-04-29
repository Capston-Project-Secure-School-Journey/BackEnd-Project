using Api.Domain.Models;

namespace Api.Services.ApplicationService;

public interface IApplicationService
{
    Task<List<DriverApprovalRequest>> GetApplicationsBySchool(Guid schoolId);
    Task<List<DriverApprovalRequest>> GetApplicationsByDriver(Guid driverId);
    Task<DriverApprovalRequest> GetApplication(Guid applicationId);
}
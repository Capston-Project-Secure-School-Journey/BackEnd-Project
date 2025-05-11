using Api.DTOs.ApprovalProcessor;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.ApplicationService;

public interface IApplicationHandler
{
    Task<Pagination<ApplicationResponse>> GetApplicationsBySchool(Guid schoolId, GetDriverApprovalApplication request);
    Task<Pagination<ApplicationResponse>> GetApplicationsByDriver(Guid driverId, GetDriverApprovalApplication request);
    Task<ApplicationResponse> GetApplication(Guid applicationId);
    Task<ApplicationResponse> CreateApplication(Guid driverId, Guid schoolId);
    Task<ApplicationResponse> UpdateApplication(Guid applicationId, Guid driverId);
    Task SubmitApplication(Guid applicationId, Guid driverId);
    Task ApproveApplication(Guid applicationId, Guid reviewerId);
    Task RejectApplication(Guid applicationId, Guid reviewerId, string reason);
    Task RequireAdditionalDetails(Guid applicationId, Guid reviewerId, string reason);
    Task CancelApplicationByReviewer(Guid applicationId, Guid reviewerId, string reason);
    Task CancelApplicationByDriver(Guid applicationId, Guid driverId, string reason);
    Task DeleteApplicationByDriver(Guid applicationId, Guid driverId);
    Task<List<ApplicationActionDto>> GetActionCanDoByReviewer(Guid applicationId, Guid reviewerId);
    Task<List<ApplicationActionDto>> GetActionCanDoByDriver(Guid applicationId, Guid driverId);
    Task IsDriverOwnerOfApplication(Guid applicationId, Guid driverId);
    Task IsSchoolOwnerOfApplication(Guid applicationId, Guid schoolId);
}
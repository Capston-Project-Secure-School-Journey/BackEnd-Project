using Api.Domain.Models;
using Api.DTOs.ApprovalProcessor;

namespace Api.Services.ApprovalProcessor;

public interface IApprovalProcessor
{
    Task<DriverApprovalRequest> CreateApplication(Guid driverId, Guid schoolId);
    Task<DriverApprovalRequest> UpdateApplication(Guid applicationId, Guid driverId);
    Task SubmitApplication(Guid applicationId, Guid driverId);
    Task ApproveApplication(Guid applicationId, Guid reviewerId);
    Task RejectApplication(Guid applicationId, Guid reviewerId, string reason);
    Task RequireAdditionalDetails(Guid applicationId, Guid reviewerId, string reason);
    Task CancelApplicationByReviewer(Guid applicationId, Guid reviewerId, string reason);
    Task CancelApplicationByDriver(Guid applicationId, Guid driverId, string reason);
    Task DeleteApplicationByDriver(Guid applicationId, Guid driverId);
    Task<List<ApplicationActionDto>> GetActionCanDoByReviewer(Guid applicationId, Guid reviewerId);
    Task<List<ApplicationActionDto>> GetActionCanDoByDriver(Guid applicationId, Guid driverId);
    Task<Guid> GetReviewerOfSchool(Guid schoolId);
}
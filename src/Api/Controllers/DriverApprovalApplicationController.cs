using Api.Attributes;
using Api.Common.Enums;
using Api.DTOs.ApprovalProcessor;
using Api.Services.ApplicationService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("driver-approval-applications")]
public class DriverApprovalApplicationController(IApplicationHandler applicationHandler) : ControllerBase
{
    private readonly IApplicationHandler _applicationHandler = applicationHandler;

    [HttpGet("{applicationId}")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<ApplicationResponse> GetApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
        {
            await _applicationHandler.IsDriverOwnerOfApplication(applicationId, userId);
        }
        else
        {
            var schoolId = this.GetSchoolId();
            await _applicationHandler.IsSchoolOwnerOfApplication(applicationId, schoolId);
        }

        return await _applicationHandler.GetApplication(applicationId);
    }

    [HttpGet]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<Pagination<ApplicationResponse>> GetApplications([FromQuery] GetDriverApprovalApplication request)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
        {
            return await _applicationHandler.GetApplicationsByDriver(userId, request);
        }
        else
        {
            var schoolId = this.GetSchoolId();
            return await _applicationHandler.GetApplicationsBySchool(schoolId, request);
        }
    }

    [HttpGet("{applicationId}/action-can-do")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<List<ApplicationActionDto>> GetActionsCanDo([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
            return await _applicationHandler.GetActionCanDoByDriver(applicationId, userId);
        else
            return await _applicationHandler.GetActionCanDoByReviewer(applicationId, userId);
    }

    [HttpPost]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ApplicationResponse> CreateApplication([FromBody] Guid schoolId)
    {
        var userId = this.GetUserId();
        return await _applicationHandler.CreateApplication(userId, schoolId);
    }

    [HttpPut("{applicationId}")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ApplicationResponse> UpdateApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        return await _applicationHandler.UpdateApplication(applicationId, userId);
    }

    [HttpPut("{applicationId}/submit")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ActionResult> SubmitApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        await _applicationHandler.SubmitApplication(applicationId, userId);
        return Ok();
    }

    [HttpDelete("{applicationId}")]
    [Authorize(UserType.Driver)]
    // [CheckVerifiedEmail]
    public async Task<ActionResult> DeleteApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        await _applicationHandler.DeleteApplicationByDriver(applicationId, userId);
        return Ok();
    }

    [HttpPut("{applicationId}/reject")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> RejectApplication([FromRoute] Guid applicationId, [FromBody] RejectApplicationRequest request)
    {
        var userId = this.GetUserId();
        await _applicationHandler.RejectApplication(applicationId, userId, request.Reason);
        return Ok();
    }

    [HttpPut("{applicationId}/approve")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> ApproveApplication([FromRoute] Guid applicationId, [FromBody] ApproveApplicationRequest request)
    {
        var userId = this.GetUserId();
        await _applicationHandler.ApproveApplication(applicationId, userId);
        return Ok();
    }

    [HttpPut("{applicationId}/request-more-info")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> RequestMoreInfo([FromRoute] Guid applicationId, [FromBody] RequestMoreInfoRequest request)
    {
        var userId = this.GetUserId();
        await _applicationHandler.RequireAdditionalDetails(applicationId, userId, request.Reason);
        return Ok();
    }

    [HttpPut("{applicationId}/cancel")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> CancelApplication([FromRoute] Guid applicationId, [FromBody] CancelApplicationRequest request)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();
        if (userType == UserType.Driver)
            await _applicationHandler.CancelApplicationByDriver(applicationId, userId, request.Reason);
        else
            await _applicationHandler.CancelApplicationByReviewer(applicationId, userId, request.Reason);
        return Ok();
    }
}
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
    [HttpGet("{applicationId}")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<ApplicationResponse> GetApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
        {
            await applicationHandler.IsDriverOwnerOfApplication(applicationId, userId);
        }
        else
        {
            var schoolId = this.GetSchoolId();
            await applicationHandler.IsSchoolOwnerOfApplication(applicationId, schoolId);
        }

        return await applicationHandler.GetApplication(applicationId);
    }

    [HttpGet]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<Pagination<ApplicationResponse>> GetApplications([FromQuery] GetDriverApprovalApplication request)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
        {
            return await applicationHandler.GetApplicationsByDriver(userId, request);
        }
        else
        {
            var schoolId = this.GetSchoolId();
            return await applicationHandler.GetApplicationsBySchool(schoolId, request);
        }
    }

    [HttpGet("{applicationId}/action-can-do")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    public async Task<List<ApplicationActionDto>> GetActionsCanDo([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();

        if (userType == UserType.Driver)
            return await applicationHandler.GetActionCanDoByDriver(applicationId, userId);
        else
            return await applicationHandler.GetActionCanDoByReviewer(applicationId, userId);
    }

    [HttpPost]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ApplicationResponse> CreateApplication([FromBody] Guid schoolId)
    {
        var userId = this.GetUserId();
        return await applicationHandler.CreateApplication(userId, schoolId);
    }

    [HttpPut("{applicationId}")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ApplicationResponse> UpdateApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        return await applicationHandler.UpdateApplication(applicationId, userId);
    }

    [HttpPut("{applicationId}/submit")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<ActionResult> SubmitApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        await applicationHandler.SubmitApplication(applicationId, userId);
        return Ok();
    }

    [HttpDelete("{applicationId}")]
    [Authorize(UserType.Driver)]
    // [CheckVerifiedEmail]
    public async Task<ActionResult> DeleteApplication([FromRoute] Guid applicationId)
    {
        var userId = this.GetUserId();
        await applicationHandler.DeleteApplicationByDriver(applicationId, userId);
        return Ok();
    }

    [HttpPut("{applicationId}/reject")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> RejectApplication([FromRoute] Guid applicationId,
        [FromBody] RejectApplicationRequest request)
    {
        var userId = this.GetUserId();
        await applicationHandler.RejectApplication(applicationId, userId, request.Reason);
        return Ok();
    }

    [HttpPut("{applicationId}/approve")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> ApproveApplication([FromRoute] Guid applicationId,
        [FromBody] ApproveApplicationRequest request)
    {
        var userId = this.GetUserId();
        await applicationHandler.ApproveApplication(applicationId, userId);
        return Ok();
    }

    [HttpPut("{applicationId}/request-more-info")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> RequestMoreInfo([FromRoute] Guid applicationId,
        [FromBody] RequestMoreInfoRequest request)
    {
        var userId = this.GetUserId();
        await applicationHandler.RequireAdditionalDetails(applicationId, userId, request.Reason);
        return Ok();
    }

    [HttpPut("{applicationId}/request-cancel")]
    [Authorize(UserType.Driver, UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> RequestCancellation([FromRoute] Guid applicationId,
        [FromBody] CancelApplicationRequest request)
    {
        var userId = this.GetUserId();
        var userType = this.GetUserType();
        if (userType == UserType.Driver)
            await applicationHandler.RequestCancellationByDriver(applicationId, userId, request.Reason);
        else
            await applicationHandler.RequestCancellationByReviewer(applicationId, userId, request.Reason);
        return Ok();
    }

    [HttpPut("{applicationId}/cancel")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> CancelApplication([FromRoute] Guid applicationId,
        [FromBody] CancelApplicationRequest request)
    {
        var userId = this.GetUserId();
        await applicationHandler.CancelApplication(applicationId, userId);
        return Ok();
    }
}
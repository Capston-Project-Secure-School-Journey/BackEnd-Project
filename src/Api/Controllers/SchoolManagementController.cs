using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.DTOs.Responses;
using Api.Services.SchoolManagement;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("schools")]
public class SchoolManagementController : ControllerBase
{
    private readonly ISchoolManagementHandler _schoolManagementHandler;

    public SchoolManagementController(ISchoolManagementHandler schoolManagementHandler)
    {
        _schoolManagementHandler = schoolManagementHandler;
    }

    [HttpPost]
    [Authorize(UserType.Admin)]
    [ValidateModel]
    public async Task<ActionResult<SchoolDetailResponse>> CreateSchool([FromBody] CreateSchoolRequest request)
    {
        return await _schoolManagementHandler.CreateSchool(request);
    }

    [HttpPut("{schoolId}")]
    [Authorize(UserType.Admin, UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<SchoolDetailResponse>> UpdateSchool([FromRoute] Guid schoolId,
        [FromBody] UpdateSchoolRequest request)
    {
        return await _schoolManagementHandler.UpdateSchool(schoolId, request,
            this.GetUserId(),
            this.GetUserType());
    }
    
    [HttpDelete("{schoolId}")]
    [Authorize(UserType.Admin)]
    public async Task<IActionResult> DeleteSchool([FromRoute] Guid schoolId)
    {
        await _schoolManagementHandler.DeleteSchool(schoolId);
        return Ok();
    }
    
    [HttpDelete("")]
    [Authorize(UserType.Admin)]
    public async Task<IActionResult> DeleteSchool([FromBody] List<Guid> schoolIds)
    {
        await _schoolManagementHandler.DeleteSchool(schoolIds);
        return Ok();
    }
    
    
    [HttpGet]
    [Authorize(UserType.Admin)]
    public async Task<Pagination<SchoolResponse>> GetSchools([FromQuery]GetSchoolRequest request)
    {
        return await _schoolManagementHandler.GetSchools(request);
    }
    
    [HttpGet("{schoolId}")]
    [Authorize(UserType.Admin)]
    public async Task<SchoolDetailResponse> GetSchool([FromRoute]Guid schoolId)
    {
        return await _schoolManagementHandler.GetSchool(schoolId);
    }
    
    [HttpPost("{schoolId}/change-school-admin-password")]
    [Authorize(UserType.Admin)]
    [ValidateModel]
    public async Task<ActionResult> ChangeSchoolAdminPassword([FromRoute]Guid schoolId, [FromForm] [PasswordStrength] string newPassword)
    {
        await _schoolManagementHandler.ChangeSchoolAdminPassword(schoolId, newPassword);
        return Ok();
    }
    
    // [HttpGet()]
    // [Authorize(UserType.Admin)]
    // public async Task<ActionResult<string>> GetPreSignedUploadImage()
    // {
    //     
    // }
}
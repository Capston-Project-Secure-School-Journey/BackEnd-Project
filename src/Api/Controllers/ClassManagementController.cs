using Api.Attributes;
using Api.Common.Enums;
using Api.Services.ClassManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassManagementController(IClassManagementHandler classManagementHandler)
    : ControllerBase
{
    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<ClassDetailResponse>> CreateClass([FromBody] CreateClassRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await classManagementHandler.AddClass(schoolId, request);
    }

    [HttpPut("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<ClassDetailResponse>> UpdateClass([FromRoute] Guid classId,
        [FromBody] UpdateClassRequest request)
    {
        request.Id = classId;
        var schoolId = this.GetSchoolId();
        return await classManagementHandler.UpdateClass(schoolId, request);
    }

    [HttpGet("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<ClassDetailResponse>> GetClass([FromRoute] Guid classId)
    {
        var schoolId = this.GetSchoolId();
        return await classManagementHandler.GetClassById(schoolId, classId);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<ClassResponse>>> GetClasses([FromQuery] GetClassesRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await classManagementHandler.GetClasses(schoolId, request);
    }

    [HttpDelete("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClass([FromRoute] Guid classId)
    {
        var schoolId = this.GetSchoolId();
        await classManagementHandler.DeleteClass(schoolId, classId);

        return Ok();
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<IActionResult> DeleteClasses([FromBody] List<Guid> classIds)
    {
        var schoolId = this.GetSchoolId();
        await classManagementHandler.DeleteClass(schoolId, classIds);

        return Ok();
    }
}
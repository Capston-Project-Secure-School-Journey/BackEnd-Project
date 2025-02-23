using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.DTOs.Responses;
using Api.Services.ClassManagementService;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassManagementController: ControllerBase
{
    private readonly IClassManagementHandler _classManagementHandler;
    private readonly Context _context;
    public ClassManagementController(IClassManagementHandler classManagementHandler, Context context)
    {
        _classManagementHandler = classManagementHandler;
        _context = context;
    }
    
    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<ClassResponse>> CreateClass([FromBody] CreateClassRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _classManagementHandler.AddClass(schoolId, request);
    }
    
    [HttpPut("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<ClassResponse>> UpdateClass([FromRoute]Guid classId, [FromBody] UpdateClassRequest request)
    {
        request.Id = classId;
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _classManagementHandler.UpdateClass(schoolId, request);
    }
    
    [HttpGet("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<ClassResponse>> GetClass([FromRoute]Guid classId)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _classManagementHandler.GetClassById(schoolId, classId);
    }
    
    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<ClassResponse>>> GetClasses([FromQuery] GetClassesRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _classManagementHandler.GetClasses(schoolId, request);
    }
    
    [HttpDelete("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClass([FromRoute]Guid classId)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        await _classManagementHandler.DeleteClass(schoolId, classId);

        return Ok();
    }
    
    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClasses([FromBody]List<Guid> classIds)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        await _classManagementHandler.DeleteClass(schoolId, classIds);

        return Ok();
    }
}
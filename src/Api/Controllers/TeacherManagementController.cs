using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.DTOs.Responses;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("teachers")]
public class TeacherManagementController: ControllerBase
{
    private readonly ITeacherManagementHandler _teacherManagementHandler;

    public TeacherManagementController(ITeacherManagementHandler teacherManagementHandler)
    {
        _teacherManagementHandler = teacherManagementHandler;
    }
    
    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherResponse>> CreateTeacher([FromBody] CreateTeacherRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        return await _teacherManagementHandler.AddTeacher(schoolId, request);
    }
    
    [HttpPut("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherResponse>> UpdateTeacher([FromRoute]Guid teacherId, [FromBody] UpdateTeacherRequest request)
    {
        request.Id = teacherId;
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        return await _teacherManagementHandler.UpdateTeacher(schoolId, request);
    }
    
    [HttpGet("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<TeacherResponse>> GetTeacher([FromRoute]Guid teacherId)
    {
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        return await _teacherManagementHandler.GetTeacherById(schoolId, teacherId);
    }
    
    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<TeacherResponse>>> GetTeachers([FromQuery] GetTeacherRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        return await _teacherManagementHandler.GetTeachers(schoolId, request);
    }
    
    [HttpDelete("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromRoute]Guid teacherId)
    {
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        await _teacherManagementHandler.DeleteTeacher(schoolId, teacherId);

        return Ok();
    }
    
    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromBody]List<Guid> teacherIds)
    {
        var userId = this.GetUserId();
        var schoolId = await _teacherManagementHandler.GetSchoolIdBySchoolAdminId(userId);
        await _teacherManagementHandler.DeleteTeacher(schoolId, teacherIds);

        return Ok();
    }
}
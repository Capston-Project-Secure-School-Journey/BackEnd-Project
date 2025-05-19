using Api.Attributes;
using Api.Common.Enums;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("teachers")]
public class TeacherManagementController(ITeacherManagementHandler teacherManagementHandler) : ControllerBase
{
    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherDetailResponse>> CreateTeacher([FromBody] CreateTeacherRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await teacherManagementHandler.AddTeacher(schoolId, request);
    }

    [HttpPut("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherDetailResponse>> UpdateTeacher([FromRoute] Guid teacherId,
        [FromBody] UpdateTeacherRequest request)
    {
        request.Id = teacherId;
        var schoolId = this.GetSchoolId();
        return await teacherManagementHandler.UpdateTeacher(schoolId, request);
    }

    [HttpGet("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<TeacherDetailResponse>> GetTeacher([FromRoute] Guid teacherId)
    {
        var schoolId = this.GetSchoolId();
        return await teacherManagementHandler.GetTeacherById(schoolId, teacherId);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<TeacherResponse>>> GetTeachers([FromQuery] GetTeacherRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await teacherManagementHandler.GetTeachers(schoolId, request);
    }

    [HttpDelete("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromRoute] Guid teacherId)
    {
        var schoolId = this.GetSchoolId();
        await teacherManagementHandler.DeleteTeacher(schoolId, teacherId);

        return Ok();
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromBody] List<Guid> teacherIds)
    {
        var schoolId = this.GetSchoolId();
        await teacherManagementHandler.DeleteTeacher(schoolId, teacherIds);

        return Ok();
    }

    [HttpPost("{teacherId}/upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> UploadAvatar([FromRoute] Guid teacherId,
        [AllowedFile([
            ContentType.ImagePng,
            ContentType.ImageJpeg, ContentType.ImageJpg,
            ContentType.ImageHeic, ContentType.ImageHeics, ContentType.ImageHeif
        ], 10)]
        IFormFile file)
    {
        var schoolId = this.GetSchoolId();
        return Ok(await teacherManagementHandler.UploadAvatar(schoolId, teacherId, file));
    }
}
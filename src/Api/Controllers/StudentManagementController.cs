using Api.Attributes;
using Api.Common.Enums;
using Api.Services.StudentManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("students")]
public class StudentManagementController(IStudentManagementHandler studentManagementHandler) : ControllerBase
{
    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<StudentDetailResponse>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await studentManagementHandler.AddStudent(schoolId, request);
    }

    [HttpPut("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<StudentDetailResponse>> UpdateStudent([FromRoute] Guid studentId,
        [FromBody] UpdateStudentRequest request)
    {
        var schoolId = this.GetSchoolId();

        request.Id = studentId;
        return await studentManagementHandler.UpdateStudent(schoolId, request);
    }

    [HttpDelete("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteStudent([FromRoute] Guid studentId)
    {
        var schoolId = this.GetSchoolId();

        await studentManagementHandler.DeleteStudent(schoolId, studentId);
        return Ok();
    }

    [HttpDelete("")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteStudent([FromBody] List<Guid> studentIds)
    {
        var schoolId = this.GetSchoolId();

        await studentManagementHandler.DeleteStudent(schoolId, studentIds);
        return Ok();
    }


    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<Pagination<StudentResponse>> GetStudents([FromQuery] GetStudentRequest request)
    {
        var schoolId = this.GetSchoolId();

        return await studentManagementHandler.GetStudents(schoolId, request);
    }

    [HttpGet("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<StudentDetailResponse> GetStudent([FromRoute] Guid studentId)
    {
        var schoolId = this.GetSchoolId();
        return await studentManagementHandler.GetStudentById(schoolId, studentId);
    }

    [HttpPost("{studentId}/upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> UploadAvatar([FromRoute] Guid studentId,
        [AllowedFile([
            ContentType.ImagePng,
            ContentType.ImageJpeg, ContentType.ImageJpg,
            ContentType.ImageHeic, ContentType.ImageHeics, ContentType.ImageHeif
        ], 10)]
        IFormFile file)
    {
        var schoolId = this.GetSchoolId();
        return Ok(await studentManagementHandler.UploadAvatar(schoolId, studentId, file));
    }
}
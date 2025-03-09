using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.Services.StudentManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("students")]
public class StudentManagementController: ControllerBase
{
    private readonly IStudentManagementHandler _studentManagementHandler;
    private readonly Context _context;
    
    public StudentManagementController(IStudentManagementHandler studentManagementHandler,
        Context context)
    {
        _studentManagementHandler = studentManagementHandler;
        _context = context;
    }

    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<StudentDetailResponse>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _studentManagementHandler.AddStudent(schoolId, request);
    }

    [HttpPut("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<StudentDetailResponse>> UpdateStudent([FromRoute] Guid studentId,
        [FromBody] UpdateStudentRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        
        request.Id = studentId;
        return await _studentManagementHandler.UpdateStudent(schoolId, request);
    }
    
    [HttpDelete("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteStudent([FromRoute] Guid studentId)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        
        await _studentManagementHandler.DeleteStudent(schoolId, studentId);
        return Ok();
    }
    
    [HttpDelete("")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteStudent([FromBody] List<Guid> studentIds)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        
        await _studentManagementHandler.DeleteStudent(schoolId, studentIds);
        return Ok();
    }
    
    
    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<Pagination<StudentResponse>> GetStudents([FromQuery]GetStudentRequest request)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        
        return await _studentManagementHandler.GetStudents(schoolId, request);
    }
    
    [HttpGet("{studentId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<StudentDetailResponse> GetStudent([FromRoute]Guid studentId)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _studentManagementHandler.GetStudentById(schoolId, studentId);
    }
    
    [HttpPost("{studentId}/upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> UploadAvatar([FromRoute] Guid studentId,
        [AllowedFile([ContentTypeEnum.ImagePng,
            ContentTypeEnum.ImageJpeg, ContentTypeEnum.ImageJpg], 5)]IFormFile file)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return Ok(await _studentManagementHandler.UploadAvatar(schoolId, studentId, file));
    }
}
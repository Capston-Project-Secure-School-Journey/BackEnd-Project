using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.DTOs;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("teachers")]
public class TeacherManagementController : ControllerBase
{
    private readonly ITeacherManagementHandler _teacherManagementHandler;
    private readonly Context _context;

    public TeacherManagementController(ITeacherManagementHandler teacherManagementHandler,
        Context context)
    {
        _teacherManagementHandler = teacherManagementHandler;
        _context = context;
    }

    [HttpPost]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherDetailResponse>> CreateTeacher([FromBody] CreateTeacherRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await _teacherManagementHandler.AddTeacher(schoolId, request);
    }

    [HttpPut("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<TeacherDetailResponse>> UpdateTeacher([FromRoute] Guid teacherId,
        [FromBody] UpdateTeacherRequest request)
    {
        request.Id = teacherId;
        var schoolId = this.GetSchoolId();
        return await _teacherManagementHandler.UpdateTeacher(schoolId, request);
    }

    [HttpGet("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<TeacherDetailResponse>> GetTeacher([FromRoute] Guid teacherId)
    {
        var schoolId = this.GetSchoolId();
        return await _teacherManagementHandler.GetTeacherById(schoolId, teacherId);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<TeacherResponse>>> GetTeachers([FromQuery] GetTeacherRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await _teacherManagementHandler.GetTeachers(schoolId, request);
    }

    [HttpDelete("{teacherId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromRoute] Guid teacherId)
    {
        var schoolId = this.GetSchoolId();
        await _teacherManagementHandler.DeleteTeacher(schoolId, teacherId);

        return Ok();
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteTeacher([FromBody] List<Guid> teacherIds)
    {
        var schoolId = this.GetSchoolId();
        await _teacherManagementHandler.DeleteTeacher(schoolId, teacherIds);

        return Ok();
    }

    [HttpPost("{teacherId}/upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> UploadAvatar([FromRoute] Guid teacherId,
        [AllowedFile([
            ContentTypeEnum.ImagePng,
            ContentTypeEnum.ImageJpeg, ContentTypeEnum.ImageJpg,
            ContentTypeEnum.ImageHeic, ContentTypeEnum.ImageHeics, ContentTypeEnum.ImageHeif
        ], 10)]
        IFormFile file)
    {
        var schoolId = this.GetSchoolId();
        return Ok(await _teacherManagementHandler.UploadAvatar(schoolId, teacherId, file));
    }

    [HttpGet("teacher-combobox")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetTeacherCombobox([FromQuery] string name)
    {
        var schoolId = this.GetSchoolId();
        var classCombobox = await _context.Teachers
            .Where(cl => schoolId == cl.SchoolId)
            .OrderBy(x => x.FullName)
            .Select(x => new ComboBoxItem() { Id = x.Id, Name = x.FullName })
            .ToListAsync();
        if (!string.IsNullOrEmpty(name)) classCombobox = classCombobox
            .Where(x => x.Name.ToLower().Contains(name.ToLower()))
            .ToList();
        return classCombobox;
    }
}
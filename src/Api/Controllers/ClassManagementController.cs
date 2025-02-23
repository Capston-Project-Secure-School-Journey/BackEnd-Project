using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.DTOs;
using Api.DTOs.Responses;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassManagementController : ControllerBase
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
    public async Task<ActionResult<ClassResponse>> UpdateClass([FromRoute] Guid classId,
        [FromBody] UpdateClassRequest request)
    {
        request.Id = classId;
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        return await _classManagementHandler.UpdateClass(schoolId, request);
    }

    [HttpGet("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<ClassResponse>> GetClass([FromRoute] Guid classId)
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
    public async Task<IActionResult> DeleteClass([FromRoute] Guid classId)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        await _classManagementHandler.DeleteClass(schoolId, classId);

        return Ok();
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClasses([FromBody] List<Guid> classIds)
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        await _classManagementHandler.DeleteClass(schoolId, classIds);

        return Ok();
    }

    [HttpGet("grades")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetGrades()
    {
        var userId = this.GetUserId();
        var schoolId = this.GetSchoolId(_context, userId);
        var schoolType = _context.Schools.FirstOrDefault(school => school.Id == schoolId)!.SchoolType;

        var data = EnumExtension.GetComboBoxItems<Grade>();
        switch (schoolType)
        {
            case SchoolType.Preschool:
                data = data.Where(g => Convert.ToInt16(g.Value) >= 0 && Convert.ToInt16(g.Value) <= 2).ToList();
                break;
            case SchoolType.PrimarySchool:
                data = data.Where(g => Convert.ToInt16(g.Value) >= 3  && Convert.ToInt16(g.Value) <= 7).ToList();
                break;
            case SchoolType.MiddleSchool:
                data = data.Where(g => Convert.ToInt16(g.Value) >= 8 && Convert.ToInt16(g.Value) <= 11).ToList();
                break;
            case SchoolType.HighSchool:
                data = data.Where(g => Convert.ToInt16(g.Value) >= 12 && Convert.ToInt16(g.Value) <= 14).ToList();
                break;
        }

        return data;
    }
}
using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Domain;
using Api.DTOs;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<ClassDetailResponse>> CreateClass([FromBody] CreateClassRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await _classManagementHandler.AddClass(schoolId, request);
    }

    [HttpPut("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<ClassDetailResponse>> UpdateClass([FromRoute] Guid classId,
        [FromBody] UpdateClassRequest request)
    {
        request.Id = classId;
        var schoolId = this.GetSchoolId();
        return await _classManagementHandler.UpdateClass(schoolId, request);
    }

    [HttpGet("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<ClassDetailResponse>> GetClass([FromRoute] Guid classId)
    {
        var schoolId = this.GetSchoolId();
        return await _classManagementHandler.GetClassById(schoolId, classId);
    }

    [HttpGet]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<Pagination<ClassResponse>>> GetClasses([FromQuery] GetClassesRequest request)
    {
        var schoolId = this.GetSchoolId();
        return await _classManagementHandler.GetClasses(schoolId, request);
    }

    [HttpDelete("{classId}")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClass([FromRoute] Guid classId)
    {
        var schoolId = this.GetSchoolId();
        await _classManagementHandler.DeleteClass(schoolId, classId);

        return Ok();
    }

    [HttpDelete]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<IActionResult> DeleteClasses([FromBody] List<Guid> classIds)
    {
        var schoolId = this.GetSchoolId();
        await _classManagementHandler.DeleteClass(schoolId, classIds);

        return Ok();
    }

    [HttpGet("grades")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetGrades()
    {
        var schoolId = this.GetSchoolId();
        var schoolType = (await _context.Schools.FirstOrDefaultAsync(school => school.Id == schoolId))!.SchoolType;

        var data = EnumExtension.GetComboBoxItems<Grade>();
        switch (schoolType)
        {
            case SchoolType.Preschool:
                data = data.Where(g => g.Id is >= 0 and <= 2).ToList();
                break;
            case SchoolType.PrimarySchool:
                data = data.Where(g => g.Id is >= 3 and <= 7).ToList();
                break;
            case SchoolType.MiddleSchool:
                data = data.Where(g => g.Id is >= 8 and <= 11).ToList();
                break;
            case SchoolType.HighSchool:
                data = data.Where(g => g.Id is >= 12 and <= 14).ToList();
                break;
        }

        return data;
    }
}
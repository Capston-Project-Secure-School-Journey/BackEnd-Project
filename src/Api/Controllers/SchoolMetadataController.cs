using Api.Attributes;
using Api.Common.Enums;
using Api.Domain;
using Api.DTOs;
using Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("school-metadata")]
public class SchoolMetadataController(Context context) : ControllerBase
{
    [HttpGet("schools")]
    [Authorize(UserType.Driver, UserType.Parent)]
    public async Task<ActionResult> SearchSchools([FromQuery] string schoolName)
    {
        var query = context.Schools
            .AsQueryable()
            .AsNoTracking();
        
        var schools = await query
            .AsNoTracking()
            .OrderBy(sc => sc.SchoolName)
            .Select(sc => new
            {
                sc.SchoolName,
                sc.Address,
                sc.SchoolType,
                SchoolTypeName = sc.SchoolType.GetEnumDisplayName(),
                sc.Id,
            })
            .ToListAsync();

        if (!string.IsNullOrEmpty(schoolName))
            schools = schools
                .Where(x => x.SchoolName.Contains(schoolName, StringComparison.OrdinalIgnoreCase))
                .ToList();

        return Ok(schools);
    }

    [HttpGet("grades")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetGrades()
    {
        var schoolId = this.GetSchoolId();
        var schoolType = (await context.Schools.FirstOrDefaultAsync(school => school.Id == schoolId))!.SchoolType;

        var data = EnumExtension.GetComboBoxItems<Grade>();
        switch (schoolType)
        {
            case SchoolType.Preschool:
                data = data.Where(g => Convert.ToInt16(g.Id) is >= 0 and <= 2).ToList();
                break;
            case SchoolType.PrimarySchool:
                data = data.Where(g => Convert.ToInt16(g.Id) is >= 3 and <= 7).ToList();
                break;
            case SchoolType.MiddleSchool:
                data = data.Where(g => Convert.ToInt16(g.Id) is >= 8 and <= 11).ToList();
                break;
            case SchoolType.HighSchool:
                data = data.Where(g => Convert.ToInt16(g.Id) is >= 12 and <= 14).ToList();
                break;
        }

        return data;
    }

    [HttpGet("classes")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetClassCombobox([FromQuery] string name)
    {
        var schoolId = this.GetSchoolId();
        var classCombobox = await context.Classes
            .Where(cl => schoolId == cl.SchoolId)
            .OrderBy(x => x.Grade)
            .ThenBy(x => x.ClassName)
            .Select(x => new ComboBoxItem() { Id = x.Id, Name = x.ClassName })
            .ToListAsync();
        if (!string.IsNullOrEmpty(name))
            classCombobox = classCombobox
                .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        return classCombobox;
    }

    [HttpGet("teachers")]
    [Authorize(UserType.SchoolAdmin)]
    public async Task<ActionResult<List<ComboBoxItem>>> GetTeacherCombobox([FromQuery] string name)
    {
        var schoolId = this.GetSchoolId();
        var classCombobox = await context.Teachers
            .Where(cl => schoolId == cl.SchoolId)
            .Select(x => new ComboBoxItem() { Id = x.Id, Name = x.FullName })
            .ToListAsync();
        if (!string.IsNullOrEmpty(name))
            classCombobox = classCombobox
                .Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        return classCombobox;
    }
}
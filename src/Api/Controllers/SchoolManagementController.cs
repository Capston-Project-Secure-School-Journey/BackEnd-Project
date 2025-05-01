using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.SchoolManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("schools")]
public class SchoolManagementController : ControllerBase
{
    private readonly ISchoolManagementHandler _schoolManagementHandler;
    private readonly Context _context;

    public SchoolManagementController(ISchoolManagementHandler schoolManagementHandler,
        Context context)
    {
        _schoolManagementHandler = schoolManagementHandler;
        _context = context;
    }

    [HttpPost]
    [Authorize(UserType.Admin)]
    [ValidateModel]
    public async Task<ActionResult<SchoolDetailResponse>> CreateSchool([FromBody] CreateSchoolRequest request)
    {
        return await _schoolManagementHandler.CreateSchool(request);
    }

    [HttpPut("{schoolId}")]
    [Authorize(UserType.Admin, UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult<SchoolDetailResponse>> UpdateSchool([FromRoute] Guid schoolId,
        [FromBody] UpdateSchoolRequest request)
    {
        return await _schoolManagementHandler.UpdateSchool(schoolId, request,
            this.GetUserId(),
            this.GetUserType());
    }

    [HttpDelete("{schoolId}")]
    [Authorize(UserType.Admin)]
    public async Task<IActionResult> DeleteSchool([FromRoute] Guid schoolId)
    {
        await _schoolManagementHandler.DeleteSchool(schoolId);
        return Ok();
    }

    [HttpDelete("")]
    [Authorize(UserType.Admin)]
    public async Task<IActionResult> DeleteSchool([FromBody] List<Guid> schoolIds)
    {
        await _schoolManagementHandler.DeleteSchool(schoolIds);
        return Ok();
    }


    [HttpGet]
    [Authorize(UserType.Admin)]
    public async Task<Pagination<SchoolResponse>> GetSchools([FromQuery] GetSchoolRequest request)
    {
        return await _schoolManagementHandler.GetSchools(request);
    }

    [HttpGet("{schoolId}")]
    [Authorize(UserType.Admin, UserType.SchoolAdmin)]
    public async Task<SchoolDetailResponse> GetSchool([FromRoute] Guid schoolId)
    {
        var userType = this.GetUserType();
        if (userType == UserType.SchoolAdmin && this.GetSchoolId() != schoolId)
                throw new ForbiddenException("Access Denied");
        return await _schoolManagementHandler.GetSchool(schoolId);
    }

    [HttpPost("{schoolId}/change-school-admin-password")]
    [Authorize(UserType.Admin)]
    [ValidateModel]
    public async Task<ActionResult> ChangeSchoolAdminPassword([FromRoute] Guid schoolId,
        [FromForm] [PasswordStrength] string newPassword)
    {
        await _schoolManagementHandler.ChangeSchoolAdminPassword(schoolId, newPassword);
        return Ok();
    }

    [HttpGet("search-schools")]
    [Authorize(UserType.Driver, UserType.Parent)]
    public async Task<ActionResult> SearchSchools([FromQuery] string schoolName)
    {
        var query = _context.Schools
            .AsQueryable()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(schoolName))
            query = query.Where(sc => sc.SchoolName.Contains(schoolName));

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

        return Ok(schools);
    }

    [HttpPut("{schoolId}/start-date")]
    [Authorize(UserType.SchoolAdmin)]
    [ValidateModel]
    public async Task<ActionResult> SetSchoolStartDate([FromRoute] Guid schoolId,
        [FromForm] DateOnly date)
    {
        if (this.GetSchoolId() != schoolId)
            throw new ForbiddenException("Access Denied");

        var startDate = await _context.SystemVariables
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.Name == "START_DATE");

        if (startDate == null)
        {
            await _context.SystemVariables.AddAsync(new SystemVariable()
                { Name = "START_DATE", SchoolId = schoolId, Value = date.ToString() });
        }
        else
            startDate.Value = date.ToString();

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{schoolId}/pre-signed-upload-url")]
    [ValidateModel]
    [Authorize(UserType.SchoolAdmin, UserType.Admin)]
    public async Task<PreSignedUrlResponse> GetPreSignedUploadUrl(
        [FromRoute] Guid schoolId,
        string fileName,
        long fileSize,
        string contentType)
    {
        if ((fileSize / 1024f / 1024f) > 10)
        {
            throw new BadRequestException($"File quá lớn. Yêu cầu file nhỏ hơn 10Mb");
        }

        List<string> acceptContentType =
        [
            ContentTypeEnum.ImagePng.GetDescription(),
            ContentTypeEnum.ImageJpeg.GetDescription(),
            ContentTypeEnum.ImageJpg.GetDescription(),
            ContentTypeEnum.ImageHeic.GetDescription(),
            ContentTypeEnum.ImageHeics.GetDescription(),
            ContentTypeEnum.ImageHeif.GetDescription()
        ];

        if (!acceptContentType.Contains(contentType))
        {
            throw new BadRequestException("Loại file không được chấp nhận.");
        }

        return await _schoolManagementHandler.GetPreSignedUploadImage(this.GetUserId(),
            schoolId,
            fileName,
            contentType,
            fileSize);
    }
}
using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.SchoolManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("schools")]
public class SchoolManagementController : ControllerBase
{
    private readonly ISchoolManagementHandler _schoolManagementHandler;

    public SchoolManagementController(ISchoolManagementHandler schoolManagementHandler)
    {
        _schoolManagementHandler = schoolManagementHandler;
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
            throw new ForbiddenException(ErrorMessages.AccessDenied);
        return await _schoolManagementHandler.GetSchool(schoolId);
    }

    [HttpPost("{schoolId}/change-school-admin-password")]
    [Authorize(UserType.Admin)]
    [ValidateModel]
    public async Task<ActionResult> ChangeSchoolAdminPassword([FromRoute] Guid schoolId,
        [FromBody] [PasswordStrength] string newPassword)
    {
        await _schoolManagementHandler.ChangeSchoolAdminPassword(schoolId, newPassword);
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
        if (fileSize / 1024f / 1024f > 10) throw new BadRequestException(ErrorMessages.FileTooLargeLimit(10));

        List<string> acceptContentType =
        [
            ContentType.ImagePng.GetDescription(),
            ContentType.ImageJpeg.GetDescription(),
            ContentType.ImageJpg.GetDescription(),
            ContentType.ImageHeic.GetDescription(),
            ContentType.ImageHeics.GetDescription(),
            ContentType.ImageHeif.GetDescription()
        ];

        if (!acceptContentType.Contains(contentType)) throw new BadRequestException(ErrorMessages.InvalidFileType);

        return await _schoolManagementHandler.GetPreSignedUploadImage(this.GetUserId(),
            schoolId,
            fileName,
            contentType,
            fileSize);
    }
}
using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.UserService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("me")]
public class UserController(IUserHandler userHandler) : ControllerBase
{
    [HttpGet]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<UserProfile> GetProfile()
    {
        return await userHandler.GetProfile(this.GetUserId(), this.GetUserType());
    }

    [HttpPut]
    [ValidateModel]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<UserProfile> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        return await userHandler.UpdateProfile(this.GetUserId(), request);
    }

    [HttpPost("upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<string> UploadAvatar(
        [AllowedFile([
            ContentType.ImagePng,
            ContentType.ImageJpeg, ContentType.ImageJpg,
            ContentType.ImageHeic, ContentType.ImageHeics, ContentType.ImageHeif
        ], 10)]
        IFormFile file)
    {
        return await userHandler.UpdateAvatar(this.GetUserId(), file);
    }

    [HttpPut("driver-information")]
    [ValidateModel]
    [Authorize(UserType.Driver)]
    public async Task<UserProfile> UpdateDriverInformation([FromBody] UpdateDriverInformationRequest request)
    {
        return await userHandler.UpdateDriverInformation(this.GetUserId(), request);
    }

    [HttpPut("add-device-tokens")]
    [ValidateModel]
    [Authorize()]
    public async Task<ActionResult> AddDeviceToken([FromBody] AddDeviceTokenRequest request)
    {
        await userHandler.AddDeviceToken(this.GetUserId(), request.DeviceToken);
        return Ok();
    }

    [HttpPost("pre-signed-upload-url")]
    [ValidateModel]
    [Authorize(UserType.Driver)]
    public async Task<PreSignedUrlResponse> GetPreSignedUploadUrl(
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

        return await userHandler.GetPreSignedUploadImage(this.GetUserId(), fileName, contentType, fileSize);
    }
}
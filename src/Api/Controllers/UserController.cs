using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.UserService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("me")]
public class UserController : ControllerBase
{
    private readonly IUserHandler _userHandler;

    public UserController(IUserHandler userHandler)
    {
        _userHandler = userHandler;
    }

    [HttpGet]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<UserProfile> GetProfile()
    {
        return await _userHandler.GetProfile(this.GetUserId(), this.GetUserType());
    }

    [HttpPut]
    [ValidateModel]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<UserProfile> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        return await _userHandler.UpdateProfile(this.GetUserId(), request);
    }

    [HttpPost("upload-avatar")]
    [ValidateModel]
    [Authorize(UserType.Admin, UserType.SchoolAdmin, UserType.SchoolSuperVisor, UserType.Parent, UserType.Driver)]
    public async Task<string> UploadAvatar(
        [AllowedFile([
            ContentTypeEnum.ImagePng,
            ContentTypeEnum.ImageJpeg, ContentTypeEnum.ImageJpg,
            ContentTypeEnum.ImageHeic, ContentTypeEnum.ImageHeics, ContentTypeEnum.ImageHeif
        ], 10)]
        IFormFile file)
    {
        return await _userHandler.UpdateAvatar(this.GetUserId(), file);
    }

    [HttpPost("driver-information")]
    [ValidateModel]
    [Authorize(UserType.Driver)]
    public async Task<UserProfile> UpdateDriverInformation([FromBody] UpdateDriverInformationRequest request)
    {
        return await _userHandler.UpdateDriverInformation(this.GetUserId(), request);
    }

    [HttpPost("pre-signed-upload-url")]
    [ValidateModel]
    [Authorize(UserType.Driver)]
    public async Task<PreSignedUrlResponse> GetPreSignedUploadUrl(
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
            ContentTypeEnum.ImageHeic.GetDescription(),
            ContentTypeEnum.ImageHeics.GetDescription(),
            ContentTypeEnum.ImageHeif.GetDescription()
        ];

        if (!acceptContentType.Contains(contentType))
        {
            throw new BadRequestException("Loại file không được chấp nhận.");
        }

        return await _userHandler.GetPreSignedUploadImage(this.GetUserId(), fileName, contentType, fileSize);
    }
}
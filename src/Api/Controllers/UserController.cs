using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
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
            ContentTypeEnum.ImageJpeg, ContentTypeEnum.ImageJpg
        ], 5)]
        IFormFile file)
    {
        return await _userHandler.UpdateAvatar(this.GetUserId(), file);
    }
}
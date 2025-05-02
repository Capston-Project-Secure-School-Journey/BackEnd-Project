using Api.Common.Enums;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UploadFileService;
using Api.DTOs.User;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Services.UserService;

public class UserHandler : IUserHandler
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _uploadFileService;
    private readonly Context _context;
    private readonly IUserBanService _userBanService;
    private readonly IFileUploadService _fileUploadService;

    public UserHandler(IUserService userService, IMapper mapper,
        IFileUploadService uploadFileService,
        Context context,
        IUserBanService userBanService,
        IFileUploadService fileUploadService)
    {
        _userService = userService;
        _mapper = mapper;
        _uploadFileService = uploadFileService;
        _context = context;
        _userBanService = userBanService;
        _fileUploadService = fileUploadService;
    }

    public async Task<UserProfile> GetProfile(Guid id, UserType userType)
    {
        var user = await _userService.GetUser(id, userType);
        return await MapUserToUserProfile(user);
    }

    public async Task<string> UpdateAvatar(Guid id, IFormFile file)
    {
        return await _userService.UpdateAvatar(id, file);
    }

    public async Task<UserProfile> UpdateDriverInformation(Guid id, UpdateDriverInformationRequest request)
    {
        var user = await _userService.UpdateDriverInformation(id, request);
        return await MapUserToUserProfile(user);
    }

    public async Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid userId, string fileName, string contentType,
        long fileSize)
    {
        await _userBanService.CheckUserBaned(userId, BanType.S3PreSigned, true);
        var request = new PreSignedUrlRequest()
        {
            Prefix = $"driver-images/{userId}",
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
        };
        var response = await _fileUploadService.GeneratePreSignedUploadUrlAsync(request);
        await _userBanService.AddErrorRequest(userId, BanType.S3PreSigned);
        return response;
    }

    public async Task<UserProfile> UpdateProfile(Guid id, UpdateProfileRequest request)
    {
        var dto = _mapper.Map<UpdateUserInfoDto>(request);
        var user = await _userService.UpdateUserInfo(id, dto);
        return await MapUserToUserProfile(user);
    }


    private async Task<UserProfile> MapUserToUserProfile(User user)
    {
        UserProfile? profile;
        if (UserType.SchoolAdmin == user.UserType)
        {
            var schoolPerson = user as SchoolPerson;
            profile = _mapper.Map<UserProfile>(schoolPerson!);
            var entity = _context.Entry(schoolPerson!);

            if (!entity.Reference(x => x.School).IsLoaded)
                await entity.Reference(x => x.School).LoadAsync();
            profile.SchoolName = schoolPerson!.School.SchoolName;
        }
        else if (UserType.Driver == user.UserType)
        {
            var driver = user as Driver;
            profile = _mapper.Map<UserProfile>(user as Driver);
            foreach (var i in driver!.DriverInformationImages)
            {
                var url = await _uploadFileService.GeneratePreSignedDownloadUrlAsync(i.FileManagementId);
                profile.DriverInformationImages.Add(new DriverInformationImageUrl() { Type = i.Type, Url = url });
                profile.DriverInformationImageKeys.Add(new DriverInformationImageKey()
                    { Type = i.Type, Id = i.FileManagementId });
            }

            foreach (var id in driver.VehicleImages.Select(x => x.FileManagementId))
            {
                var url = await _uploadFileService.GeneratePreSignedDownloadUrlAsync(id);
                profile.VehicleImages.Add(url);
                profile.VehicleImageKeys.Add(id);
            }
        }
        else
        {
            profile = _mapper.Map<UserProfile>(user);
        }

        if (user.AvatarKey != null)
            profile.AvatarUrl = await _uploadFileService.GeneratePreSignedDownloadUrlAsync(user.AvatarKey.Value);
        return profile;
    }
}
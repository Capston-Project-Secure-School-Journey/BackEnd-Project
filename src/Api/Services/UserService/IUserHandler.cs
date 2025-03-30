using Api.Common.Enums;
using Api.DTOs.UploadFileService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.UserService;

public interface IUserHandler
{
    Task<UserProfile> GetProfile(Guid id, UserType userType);
    Task<UserProfile> UpdateProfile(Guid id, UpdateProfileRequest request);
    Task<string> UpdateAvatar(Guid id, IFormFile file);
    Task<UserProfile> UpdateDriverInformation(Guid id, UpdateDriverInformationRequest request);
    Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid userId, string fileName, string contentType, long fileSize);
}
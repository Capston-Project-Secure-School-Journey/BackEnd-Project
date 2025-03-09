using Api.Common.Enums;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.UserService;

public interface IUserHandler
{
    Task<UserProfile> GetProfile(Guid id, UserType userType);
    Task<UserProfile> UpdateProfile(Guid id, UpdateProfileRequest request);
    Task<string> UpdateAvatar(Guid id, IFormFile file);
}
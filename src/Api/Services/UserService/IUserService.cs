using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.User;
using Api.TransferDTOs.Requests;

namespace Api.Services.UserService;

public interface IUserService
{
    Task<User> GetUser(Guid id, UserType userType);
    Task<User> UpdateUserInfo(Guid id, UpdateUserInfoDto dto);
    Task<string> UpdateAvatar(Guid id, IFormFile file);
    Task<User> UpdateDriverInformation(Guid id, UpdateDriverInformationRequest request);
}
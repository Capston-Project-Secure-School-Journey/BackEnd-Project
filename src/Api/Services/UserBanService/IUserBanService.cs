using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Services.UserBanService;

public interface IUserBanService
{
    Task<UserBan?> CheckUserBaned(Guid userId, BanType type, bool isThrowException = false);
    Task<UserBan?> AddUserBan(Guid userId, BanType type);
    Task AddErrorRequest(Guid userId, BanType type);
    Task RemoveErrorRequest(Guid userId);
    Task RemoveUserBan(Guid userId, BanType type);
}
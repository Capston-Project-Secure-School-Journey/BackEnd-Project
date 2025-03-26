using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.Extensions;

namespace Api.Services.UserBanService;

public class UserBanService : IUserBanService
{
    private readonly Context _context;

    public UserBanService(Context context)
    {
        _context = context;
    }

    public Task<UserBan?> CheckUserBaned(Guid userId, BanType type, bool isThrowException = false)
    {
        var now = DateTime.UtcNow;
        var userBan = _context.UserBans
            .FirstOrDefault(x => x.UserId == userId && x.BanExpiryDate > now
                                        && x.BanType == type);

        if (userBan != null)
        {
            if (isThrowException)
                throw new BadRequestException(userBan.Reason);

            return Task.FromResult<UserBan?>(userBan);
        }

        return Task.FromResult<UserBan?>(null);
    }

    public Task<UserBan?> AddUserBan(Guid userId, BanType type)
    {
        var limit = type.GetBanAttemptLimit();
        var timeBan = type.GetBanAttemptBanTime();
        var observationWindow = type.GetBanAttemptObservationWindow();
        
        var errorRequest = _context.UserRequestedLogs
            .Where(x => x.UserId == userId
                        && x.UserRequestedType == type
                        && x.DatetimeRequested >= DateTime.UtcNow.AddHours(observationWindow * -1)
                        && x.DatetimeRequested <= DateTime.UtcNow.AddMinutes(2));

        var count = errorRequest.Count();

        if (count >= limit)
        {
            var userBan = new UserBan()
            {
                UserId = userId,
                BanType = type,
                BanDate = DateTime.UtcNow,
                BanExpiryDate = DateTime.UtcNow.AddSeconds(timeBan),
                Reason = GetReason(type)
            };
            _context.UserBans.Add(userBan);
            _context.SaveChanges();
            return Task.FromResult<UserBan?>(userBan);
        }
        return Task.FromResult<UserBan?>(null);
    }

    public async Task AddErrorRequest(Guid userId, BanType type)
    {
        var errorRequest = new UserRequestedLog()
        {
            UserId = userId,
            UserRequestedType = type,
            DatetimeRequested = DateTime.UtcNow
        };

        _context.UserRequestedLogs.Add(errorRequest);
        await _context.SaveChangesAsync();
        await AddUserBan(userId, type);
    }

    public Task RemoveErrorRequest(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserBan(Guid userId, BanType type)
    {
        var bans = _context.UserBans
            .Where(x => x.UserId == userId && x.BanType == type);
        var requestLogs = _context.UserRequestedLogs
            .Where(x => x.UserId == userId && x.UserRequestedType == type);
        _context.UserBans.RemoveRange(bans);
        _context.UserRequestedLogs.RemoveRange(requestLogs);
        return _context.SaveChangesAsync();
    }

    private string GetReason(BanType type)
    {
        var banTime = type.GetBanAttemptBanTime();
        var limit = type.GetBanAttemptLimit();
        
        switch (type)
        {
            case BanType.Login:
                return $"Bạn đã đăng nhập sai quá {limit} lần. Hãy đợi sau {ConvertSecondsToTimeString(banTime)} để đăng nhập lại";
            case BanType.S3PreSigned:
                return $"Bạn đã quá giới hạn tải file. Hãy đợi sau {ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.SendVerifyEmail:
                return $"Bạn đã yêu cầu gửi email quá {limit} lần. Hãy đợi sau {ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.SendSms:
                return $"Bạn đã yêu cầu gửi tin nhắn quá {limit} lần. Hãy đợi sau {ConvertSecondsToTimeString(banTime)} để thử lại";
            case BanType.AddChild:
                return $"Bạn đã sai quá {limit} lần khi xác thực thông tin. Hãy đợi sau {ConvertSecondsToTimeString(banTime)} để thử lại";
            default:
                return "Bạn đã bị ban";
        }
    }
    
    private string ConvertSecondsToTimeString(int totalSeconds)
    {
        int days = totalSeconds / 86400;
        int remainder = totalSeconds % 86400;
        int hours = remainder / 3600;
        remainder %= 3600;
        int minutes = remainder / 60;

        List<string> components = new List<string>();
    
        if (days > 0)
            components.Add($"{days} ngày");
        if (hours > 0)
            components.Add($"{hours} giờ");
        if (minutes > 0)
            components.Add($"{minutes} phút");
        
        if (components.Count == 0)
            return "0 phút";
        return string.Join(" ", components);
    }
}
using System.Security.Claims;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Services.TokenService;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Api.Security.CurrentUserProvider;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public CurrentUser GetCurrentUser()
    {
        if (httpContextAccessor.HttpContext == null)
            throw new InvalidOperationException("httpContextAccessor.HttpContext is null");
        try
        {
            var userId = GetSingleClaimValue(ClaimTypes.NameIdentifier);
            var userType = GetSingleClaimValue(ClaimTypes.Role);
            var accountStatus = GetSingleClaimValue(ClaimType.AccountStatus);
            var schoolId = GetSingleClaimValue(ClaimType.SchoolId);

            return new CurrentUser(
                UserId: Guid.Parse(userId ?? throw new InvalidOperationException()),
                UserType: (UserType)Convert.ToInt16(userType),
                AccountStatus: (AccountStatus)Convert.ToInt16(accountStatus),
                SchoolId: Guid.TryParse(schoolId, result: out var scId) ? scId : null);
        }
        catch (Exception e)
        {
            throw new UnauthorizedAccessException(ErrorMessages.NotLoggedIn);
        }
    }

    private string? GetSingleClaimValue(string claimType) =>
        httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(claim => claim.Type == claimType)?.Value;
}
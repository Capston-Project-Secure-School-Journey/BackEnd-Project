using Api.Common.Enums;
using Api.Services.TokenService;
using Hangfire.Dashboard;

namespace Api.DashboardAuthorizationFilters;

public class DashboardAuthorizationFilter(ITokenService tokenService) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var token = httpContext.Request.Headers.Authorization.ToString().Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(token))
            return false;

        var userInfo = tokenService.ValidateToken(token);

        if ((UserType)Convert.ToInt16(userInfo.Item2) == UserType.Admin)
            return true;
        return false;
    }
}
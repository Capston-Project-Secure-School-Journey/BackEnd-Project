using Api.Common.Enums;
using Api.Services.TokenService;
using Hangfire.Dashboard;

namespace Api.DashboardAuthorizationFilters;

public class DashboardAuthorizationFilter(ITokenService tokenService) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        try
        {
            var httpContext = context.GetHttpContext();
            var token = httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();

            if (string.IsNullOrEmpty(token))
                return false;

            var userInfo = tokenService.ValidateToken(token);

            return (UserType)Convert.ToInt16(userInfo.Item2) == UserType.Admin;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
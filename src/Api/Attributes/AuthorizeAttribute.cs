using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Services.TokenService;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(params UserType[]? userTypeFilter) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            var checker = context.HttpContext.RequestServices.GetService<IAuthorizationChecker>();
            checker!.Check(context, userTypeFilter);
        }
        catch (UnAuthorizedException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new UnAuthorizedException(ErrorMessages.NotLoggedIn);
        }
    }
}

public interface IAuthorizationChecker
{
    void Check(AuthorizationFilterContext context, UserType[]? userTypeFilter);
}

public class AuthorizationChecker(ITokenService tokenService) : IAuthorizationChecker
{
    public void Check(AuthorizationFilterContext context, UserType[]? userTypeFilter)
    {
        var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(token))
            throw new UnAuthorizedException(ErrorMessages.NotLoggedIn);

        var userInfo = tokenService.ValidateToken(token);

        if (userInfo is { Item1: not null, Item2: not null, Item3: not null })
        {
            RemoveHeader(context.HttpContext);
            AddHeader(context.HttpContext, userInfo);

            if (userInfo.Item3.Value == AccountStatus.DeActive)
                throw new ForbiddenException(ErrorMessages.AccountLocked);

            if (userTypeFilter is { Length: > 0 } &&
                !userTypeFilter.Contains((UserType)Convert.ToInt16(userInfo.Item2))
               )
                throw new ForbiddenException(ErrorMessages.AccessDenied);
        }
        else
        {
            throw new UnAuthorizedException(ErrorMessages.NotLoggedIn);
        }
    }

    private static void RemoveHeader(HttpContext context)
    {
        if (context.Request.Headers.Any(x => x.Key == "Authorization-UserId"))
            context.Request.Headers.Remove("Authorization-UserId");

        if (context.Request.Headers.Any(x => x.Key == "Authorization-UserType"))
            context.Request.Headers.Remove("Authorization-UserType");

        if (context.Request.Headers.Any(x => x.Key == "Authorization-AccountStatus"))
            context.Request.Headers.Remove("Authorization-AccountStatus");

        if (context.Request.Headers.Any(x => x.Key == "Authorization-SchoolId"))
            context.Request.Headers.Remove("Authorization-SchoolId");
    }

    private static void AddHeader(HttpContext context, (Guid?, string?, AccountStatus?, Guid? schoolId) userInfo)
    {
        context.Request.Headers.TryAdd("Authorization-UserId", userInfo.Item1?.ToString());
        context.Request.Headers.TryAdd("Authorization-UserType", userInfo.Item2);
        context.Request.Headers.TryAdd("Authorization-AccountStatus", userInfo.Item3.ToString());
        context.Request.Headers.TryAdd("Authorization-SchoolId", userInfo.Item4?.ToString());
    }
}
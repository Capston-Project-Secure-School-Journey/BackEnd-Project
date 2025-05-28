using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Security.CurrentUserProvider;
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

public class AuthorizationChecker() : IAuthorizationChecker
{
    public void Check(AuthorizationFilterContext context, UserType[]? userTypeFilter)
    {
        var currentUserProvider = context.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();

        if (user.AccountStatus == AccountStatus.DeActive)
            throw new ForbiddenException(ErrorMessages.AccountLocked);

        if (userTypeFilter is { Length: > 0 } &&
            !userTypeFilter.Contains(user.UserType)
           )
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }
}

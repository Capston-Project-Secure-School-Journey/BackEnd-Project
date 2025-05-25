using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Security.CurrentUserProvider;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CheckVerifiedEmailAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            var checker = context.HttpContext.RequestServices.GetService<IVerifiedEmailChecker>();
            checker!.Check(context);
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

public interface IVerifiedEmailChecker
{
    void Check(AuthorizationFilterContext context);
}

public class VerifiedEmailChecker : IVerifiedEmailChecker
{
    public void Check(AuthorizationFilterContext context)
    {
        var currentUserProvider = context.HttpContext.RequestServices.GetService<ICurrentUserProvider>();
        var user = currentUserProvider!.GetCurrentUser();

        if (user.AccountStatus != AccountStatus.Verified)
            throw new ForbiddenException(ErrorMessages.AccountNotVerified);
    }
}
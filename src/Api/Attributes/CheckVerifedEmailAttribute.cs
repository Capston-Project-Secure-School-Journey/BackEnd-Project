using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
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
        var accountStatus = GetAccountStatus(context.HttpContext);

        if (accountStatus != null && accountStatus.Value != AccountStatus.Verified)
            throw new ForbiddenException(ErrorMessages.AccountNotVerified);
    }

    private static AccountStatus? GetAccountStatus(HttpContext context)
    {
        var accountStatus = context.Request.Headers["Authorization-AccountStatus"].FirstOrDefault();
        if (Enum.TryParse(accountStatus, out AccountStatus status)) return status;

        return null;
    }
}
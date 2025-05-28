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

        var tokenValidationResult = tokenService.ValidateToken(token, TokenType.Login);

        if (tokenValidationResult is { UserId: not null, UserType: not null, AccountStatus: not null })
        {
            AddHeader(context.HttpContext, tokenValidationResult);

            if (tokenValidationResult.AccountStatus.Value == AccountStatus.DeActive)
                throw new ForbiddenException(ErrorMessages.AccountLocked);

            if (userTypeFilter is { Length: > 0 } &&
                !userTypeFilter.Contains(tokenValidationResult.UserType.Value)
               )
                throw new ForbiddenException(ErrorMessages.AccessDenied);
        }
        else
        {
            throw new UnAuthorizedException(ErrorMessages.NotLoggedIn);
        }
    }
    
    private static void AddHeader(HttpContext context, TokenValidationResult result)
    {
        context.Request.Headers.TryAdd("Authorization-UserId", result.UserId.ToString());
        context.Request.Headers.TryAdd("Authorization-UserType", Convert.ToInt16(result.UserType).ToString());
        context.Request.Headers.TryAdd("Authorization-AccountStatus", Convert.ToInt16(result.AccountStatus).ToString());
        context.Request.Headers.TryAdd("Authorization-SchoolId", result.SchoolId.ToString());
    }
}
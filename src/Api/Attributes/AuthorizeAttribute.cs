using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Services.TokenService;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute(bool isVerifed, params UserType[] userTypeFilter) : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var checker = context.HttpContext.RequestServices.GetService<IAuthorizationChecker>();
                checker!.Check(context, userTypeFilter, isVerifed);
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
                throw new UnAuthorizedException("Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng");
            }

        }
    }

    public interface IAuthorizationChecker
    {
        void Check(AuthorizationFilterContext context, UserType[] userTypeFilter, bool isVerifed = false);
    }
    
    public class AuthorizationChecker : IAuthorizationChecker
    {
        private readonly ITokenService _tokenService;
        public AuthorizationChecker(ITokenService tokenService) 
        {
            this._tokenService = tokenService;
        }

        public void Check(AuthorizationFilterContext context, UserType[] userTypeFilter, bool isVerifed = false) 
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnAuthorizedException("Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng");
            }

            var userInfo = _tokenService.ValidateToken(token);
                
            if (userInfo is { Item1: not null, Item2: not null, Item3: not null })
            {
                RemoveHeader(context.HttpContext);
                AddHeader(context.HttpContext, userInfo);
                
                if(isVerifed && userInfo.Item3.Value == AccountStatus.New)
                {
                    throw new ForbiddenException("Tài khoản của bạn chưa được xác thực");
                }

                if(userInfo.Item3.Value == AccountStatus.Deactive)
                {
                    throw new ForbiddenException("Tài khoản của bạn đã bị khóa");
                }

                if (userTypeFilter != null && !userTypeFilter.Contains((UserType)Convert.ToInt16(userInfo.Item2)))
                {
                    throw new ForbiddenException("Bạn không có quyền truy cập tài nguyên");
                }
            }
            else
            {
                throw new UnAuthorizedException("Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng");
            }
        }
        
        private void RemoveHeader(HttpContext context)
        {
            if (context.Request.Headers.Any(x => x.Key == "Authorization-UserId"))
            {
                context.Request.Headers.Remove("Authorization-UserId");
            }

            if (context.Request.Headers.Any(x => x.Key == "Authorization-UserType"))
            {
                context.Request.Headers.Remove("Authorization-UserType");
            }

            if (context.Request.Headers.Any(x => x.Key == "Authorization-AccountStatus"))
            {
                context.Request.Headers.Remove("Authorization-AccountStatus");
            }
        }

        private void AddHeader(HttpContext context, (Guid?, string?, AccountStatus?) userInfo)
        {
            context.Request.Headers.TryAdd("Authorization-UserId", userInfo.Item1?.ToString());
            context.Request.Headers.TryAdd("Authorization-UserType", userInfo.Item2);
            context.Request.Headers.TryAdd("Authorization-AccountStatus", userInfo.Item3.ToString());
        }
    }
}

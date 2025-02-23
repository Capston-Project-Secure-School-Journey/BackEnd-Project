using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Services.TokenService;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute(params UserType[] userTypeFilter) : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var checker = context.HttpContext.RequestServices.GetService<IAuthorizationChecker>();
                checker!.Check(context, userTypeFilter);
            }
            catch (Exception)
            {
                throw new UnAuthorizedException("Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng");
            }

        }
    }

    public interface IAuthorizationChecker
    {
        void Check(AuthorizationFilterContext context, UserType[] userTypeFilter);
    }
    
    public class AuthorizationChecker : IAuthorizationChecker
    {
        private readonly ITokenService _tokenService;
        public AuthorizationChecker(ITokenService tokenService) 
        {
            this._tokenService = tokenService;
        }

        public void Check(AuthorizationFilterContext context, UserType[] userTypeFilter) 
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                throw new UnAuthorizedException("Bạn Chưa đăng nhập. Hãy đăng nhập để tiếp tục sử dụng");
            }

            var userInfo = _tokenService.ValidateToken(token);
                
            if (userInfo is { Item1: not null, Item2: not null })
            {
                RemoveHeader(context.HttpContext);
                AddHeader(context.HttpContext, userInfo);
                
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
        }

        private void AddHeader(HttpContext context, (Guid?, string?) userInfo)
        {
            context.Request.Headers.TryAdd("Authorization-UserId", userInfo.Item1?.ToString());
            context.Request.Headers.TryAdd("Authorization-UserType", userInfo.Item2);
        }
    }
}
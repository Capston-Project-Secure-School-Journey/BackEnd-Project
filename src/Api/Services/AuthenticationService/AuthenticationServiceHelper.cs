using System.Security.Claims;
using Api.Domain.Models;
using Api.Services.TokenService;

namespace Api.Services.AuthenticationService
{
    public partial class AuthenticationService
    {
        private string GenerateLoginToken(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("Id", user.Id.ToString()));
            claims.Add(new Claim("UserName", user.UserName));
            claims.Add(new Claim(ClaimTypes.Role, Convert.ToInt16(user.UserType).ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Id.ToString()));
            claims.Add(new Claim("AccountStatus", user.AccountStatus.ToString()));
            claims.Add(new Claim("TokenType", TokenType.Login.ToString()));
            return _tokenService.GenerateAccessToken(claims);
        }
    }
}
using System.Security.Claims;
using Api.Domain.Models;
using Api.Services.TokenService;

namespace Api.Services.AuthenticationService;

public partial class AuthenticationService
{
    private string GenerateLoginToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("UserName", user.UserName),
            new Claim(ClaimTypes.Role, Convert.ToInt16(user.UserType).ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim("AccountStatus", user.AccountStatus.ToString())
        };
        if (user.VerificationMethod != null)
            claims.Add(new Claim("VerificationMethod", user.VerificationMethod.ToString()!));
        claims.Add(new Claim("TokenType", TokenType.Login.ToString()));
        if (user is SchoolPerson schoolPerson) claims.Add(new Claim("SchoolId", schoolPerson.SchoolId.ToString()));

        return tokenService.GenerateAccessToken(claims);
    }
}
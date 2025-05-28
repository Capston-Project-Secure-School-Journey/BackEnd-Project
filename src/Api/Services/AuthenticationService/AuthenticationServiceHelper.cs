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
            new(ClaimType.UserName, user.UserName),
            new(ClaimTypes.Role, Convert.ToInt16(user.UserType).ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimType.AccountStatus, Convert.ToInt16(user.AccountStatus).ToString())
        };
        if (user.VerificationMethod != null)
            claims.Add(new Claim(ClaimType.VerificationMethod, Convert.ToInt16(user.VerificationMethod).ToString()));
        claims.Add(new Claim(ClaimType.TokenType, TokenType.Login.ToString()));
        if (user is SchoolPerson schoolPerson)
            claims.Add(new Claim(ClaimType.SchoolId, schoolPerson.SchoolId.ToString()));

        return tokenService.GenerateAccessToken(claims, tokenSettings.Value.AccessTokenExpirationHours);
    }
}
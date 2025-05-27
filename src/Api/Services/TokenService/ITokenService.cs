using System.Security.Claims;

namespace Api.Services.TokenService;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims, int expireHours);

    TokenValidationResult ValidateToken(string token, TokenType type);
}
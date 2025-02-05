using System.Security.Claims;
using Api.Domain.Models;

namespace Api.Services.TokenService
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateAccessToken(User data, int expireHours = 24);
        (int?, string?) ValidateToken(string token, TokenType type = TokenType.Login);
    }
}
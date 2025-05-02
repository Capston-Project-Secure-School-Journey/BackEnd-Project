using System.Security.Claims;
using Api.Common.Enums;
using Api.Domain.Models;

namespace Api.Services.TokenService;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateAccessToken(User data, int expireHours = 48);
    (Guid?, string?, AccountStatus?, Guid? schoolId) ValidateToken(string token, TokenType type = TokenType.Login);
}
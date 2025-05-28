using System.Security.Claims;
using System.Text;
using Api.Domain.ModelSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;

namespace Api.Services.TokenService;

public enum TokenType
{
    VerifyEmail,
    ForgotEmail,
    Login
}

public static class ClaimType
{
    public const string AccountStatus = "AccountStatus";
    public const string SchoolId = "SchoolId";
    public const string TokenType = "TokenType";
    public const string UserName = "UserName";
    public const string VerificationMethod = "VerificationMethod";
}

public record TokenValidationResult(
    Guid? UserId,
    UserType? UserType,
    AccountStatus? AccountStatus,
    Guid? SchoolId,
    string Email
);

public class TokenService(IOptions<TokenSettings> tokenSettings) : ITokenService
{
    private readonly TokenSettings _tokenSettings = tokenSettings.Value;

    public string GenerateAccessToken(IEnumerable<Claim> claims, int expireHours)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
        var signinCredentials = new SigningCredentials(secretKey, _tokenSettings.SigninCredentials);

        var tokeOptions = new JwtSecurityToken(
            _tokenSettings.Issuer,
            _tokenSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return tokenString;
    }

    public TokenValidationResult ValidateToken(string token,
        TokenType type)
    {
        if (string.IsNullOrEmpty(token)) throw new UnAuthorizedException(ErrorMessages.InvalidToken);

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenValidationParameters = GetTokenValidationParameters(_tokenSettings);

        tokenHandler.ValidateToken(token,
            tokenValidationParameters
            , out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = GetClaim(jwtToken.Claims, ClaimTypes.NameIdentifier);
        var userType = GetClaim(jwtToken.Claims, ClaimTypes.Role);
        var accountStatus = GetClaim(jwtToken.Claims, ClaimType.AccountStatus);
        var schoolId = GetClaim(jwtToken.Claims, ClaimType.SchoolId);
        var email = GetClaim(jwtToken.Claims, ClaimTypes.Email);
        var tokenType = GetClaim(jwtToken.Claims, ClaimType.TokenType);

        if (tokenType == null) throw new UnAuthorizedException(ErrorMessages.InvalidToken);
        if ((TokenType)Enum.Parse(typeof(TokenType), tokenType) != type)
            throw new UnAuthorizedException(ErrorMessages.InvalidToken);

        return new TokenValidationResult(
            UserId: Guid.TryParse(userId, result: out var uid) ? uid : null,
            UserType: string.IsNullOrEmpty(userType) ? null : (UserType)Convert.ToInt16(userType),
            AccountStatus: string.IsNullOrEmpty(userType) ? null : (AccountStatus)Convert.ToInt16(accountStatus),
            SchoolId: Guid.TryParse(schoolId, result: out var scId) ? scId : null,
            Email: email
        );
    }

    private static string GetClaim(IEnumerable<Claim> claims, string key)
    {
        var value = string.Empty;
        var enumerable = claims as Claim[] ?? claims.ToArray();
        if (enumerable.Any(x => x.Type == key))
            value = enumerable.First(x => x.Type == key).Value;
        return value;
    }


    public static TokenValidationParameters GetTokenValidationParameters(TokenSettings tokenSettings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = tokenSettings.Issuer,
            ValidAudience = tokenSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key))
        };
    }
}
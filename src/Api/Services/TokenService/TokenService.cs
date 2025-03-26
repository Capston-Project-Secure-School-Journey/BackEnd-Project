using System.Security.Claims;
using System.Text;
using Api.Domain.ModelSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Api.Domain.Models;
using Api.Common.Enums;

namespace Api.Services.TokenService;

public enum TokenType
{
    VerifyEmail,
    ForgotEmail,
    Login
}

public class TokenService : ITokenService
{
    private readonly TokenSettings _tokenSettings;

    public TokenService(IOptions<TokenSettings> tokenSettings)
    {
        _tokenSettings = tokenSettings.Value;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
        var signinCredentials = new SigningCredentials(secretKey, _tokenSettings.SigninCredentials);

        var tokeOptions = new JwtSecurityToken(
            _tokenSettings.Issuer,
            _tokenSettings.Audience,
            claims,
            expires: DateTime.Now.AddHours(Convert.ToInt16(_tokenSettings.AccessTokenExpirationHours)),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return tokenString;
    }

    public string GenerateAccessToken(User data, int expireHours = 48)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
        var signinCredentials = new SigningCredentials(secretKey, _tokenSettings.SigninCredentials);
        var claims = new List<Claim>();
        claims.AddRange(new[]
        {
            new Claim("Id", data.Id.ToString()),
            new Claim("Email", data.Email),
            new Claim("Phone", data.PhoneNumber),
            new Claim("UserType", data.UserType.ToString()),
            new Claim("UserTypeName", data.UserTypeName)
        });

        var tokeOptions = new JwtSecurityToken(
            _tokenSettings.Issuer,
            _tokenSettings.Audience,
            claims,
            expires: DateTime.Now.AddHours(expireHours),
            signingCredentials: signinCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    }

    public (Guid?, string?, AccountStatus?, Guid? schoolId) ValidateToken(string token,
        TokenType type = TokenType.Login)
    {
        if (string.IsNullOrEmpty(token)) throw new Exception("Token is not valid");

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = _tokenSettings.Issuer,
            ValidAudience = _tokenSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key))
        };

        tokenHandler.ValidateToken(token,
            tokenValidationParameters
            , out var validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var userType = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value.ToString();
        var accountStatus =
            Enum.Parse<AccountStatus>(jwtToken.Claims.First(x => x.Type == "AccountStatus").Value.ToString());
        Guid? schoolId = null;
        if (jwtToken.Claims.Any(x => x.Type == "SchoolId"))
            schoolId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "SchoolId").Value);
        var typeInToken = jwtToken.Claims.First(x => x.Type == "TokenType").Value;

        if (typeInToken == null) throw new Exception("Token is not valid");

        if (typeInToken != null && (TokenType)Enum.Parse(typeof(TokenType), typeInToken) != type)
            throw new Exception("Token is not valid");

        return (userId, userType, accountStatus, schoolId);
    }
}
using Api.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.ModelSettings;
using Api.Services.TokenService;
using Api.Services.UserBanService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.Extensions.Options;

namespace Api.Services.AuthenticationService;

public partial class AuthenticationService(
    Context context,
    ITokenService tokenService,
    IUserBanService userBanService,
    IOptions<TokenSettings> tokenSettings)
    : IAuthenticationService
{
    public async Task<AuthenticateResponse> Login(AuthenticateRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user != null)
            await userBanService.CheckUserBaned(user.Id, BanType.Login, true);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            if (user != null)
                await userBanService.AddErrorRequest(user.Id, BanType.Login);

            throw new UnAuthorizedException(ErrorMessages.InvalidCredentials);
        }

        await userBanService.RemoveUserBan(user.Id, BanType.Login);

        return new AuthenticateResponse(user, GenerateLoginToken(user));
    }
}
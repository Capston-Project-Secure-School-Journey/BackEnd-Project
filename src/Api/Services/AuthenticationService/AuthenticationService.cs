using Api.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Services.TokenService;
using Api.Services.UserBanService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.AuthenticationService;

public partial class AuthenticationService : IAuthenticationService
{
    private readonly Context _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger _logger;
    private readonly IUserBanService _userBanService;

    public AuthenticationService(Context context,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger,
        IUserBanService userBanService)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
        _userBanService = userBanService;
    }

    public async Task<AuthenticateResponse> Login(AuthenticateRequest request)
    {
        try
        {
            var user = await _context.Users.Where(u => u.UserName == request.UserName).FirstOrDefaultAsync();

            if (user != null)
                await _userBanService.CheckUserBaned(user.Id, BanType.Login, true);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                if (user != null)
                    await _userBanService.AddErrorRequest(user.Id, BanType.Login);

                _logger.LogInformation("User login fail with account {@Account}", request.UserName);
                throw new UnAuthorizedException(ConstantErrorMessage.INVALID_EMAIL_PASSWORD("vn"));
            }
            
            await _userBanService.RemoveUserBan(user.Id, BanType.Login);
            
            _logger.LogInformation("User with ID = {Id} login successfully", user.Id);
            return new AuthenticateResponse(user, GenerateLoginToken(user));
        }
        catch (Exception ex)
        {
            _logger.LogInformation("User with account = {@Account} login fail. Error = {Message}", request.UserName,
                ex.Message);
            throw;
        }
    }
}
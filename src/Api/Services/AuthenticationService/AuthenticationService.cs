using Microsoft.EntityFrameworkCore;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using Api.Services.TokenService;

namespace Api.Services.AuthenticationService
{
    public partial class AuthenticationService : IAuthenticationService
    {
        private readonly Context _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        
        public AuthenticationService(Context context,
            ITokenService tokenService,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }
        
        public async Task<AuthenticateResponse> Login(AuthenticateRequest request)
        {
            try
            {
                _logger.LogInformation("User login");
                var user = await _context.Users.Where(u => u.UserName == request.UserName).FirstOrDefaultAsync();
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogInformation("User login fail with account {@Account}", request);
                    throw new UnAuthorizedException(ConstantErrorMessage.INVALID_EMAIL_PASSWORD("vn"));
                }
                _logger.LogInformation("User with ID = {Id} login successfully", user.Id);
                return new AuthenticateResponse(user, GenerateLoginToken(user));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("User with account = {@Account} login fail. Error = {Message}", request, ex.Message);
                throw;
            }
        }
    }
}

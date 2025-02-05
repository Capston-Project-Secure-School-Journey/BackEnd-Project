using Api.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Api.Services.AuthenticationService;
using Api.Transfers.Requests;
using Api.Transfers.Responses;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenController: ControllerBase
    {
        private IAuthenticationService _authenticationService;
        public AuthenController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("login")]
        [ValidateModel]
        public async Task<ActionResult<AuthenticateResponse>> Login([FromBody] AuthenticateRequest request)
        {
            return await _authenticationService.Login(request);
        }
    }
}
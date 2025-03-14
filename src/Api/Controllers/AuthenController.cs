using Api.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Api.Services.AuthenticationService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenController: ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
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
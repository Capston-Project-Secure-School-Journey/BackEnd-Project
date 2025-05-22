using Api.Attributes;
using Microsoft.AspNetCore.Mvc;
using Api.Services.AuthenticationService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthenController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("login")]
    [ValidateModel]
    public async Task<ActionResult<AuthenticateResponse>> Login([FromBody] AuthenticateRequest request)
    {
        return await authenticationService.Login(request);
    }
}
using Api.Attributes;
using Api.Services.UserManagementService;
using Api.TransferDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("users")]
[ValidateModel]
public class UserManagementController(IUserManagementHandler userManagementHandler) : ControllerBase
{
    [HttpPost("register")]
    [ValidateModel]
    public async Task<ActionResult> CreateUser([FromBody] CreateAccountRequest request)
    {
        await userManagementHandler.CreateAccount(request);
        return Ok();
    }
}
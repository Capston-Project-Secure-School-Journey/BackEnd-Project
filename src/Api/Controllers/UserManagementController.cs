using Api.Common.Utilities;
using Api.Services.UserManagementService;
using Api.TransferDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("users")]
public class UserManagementController: ControllerBase
{
    private readonly IUserManagementHandler _userManagementHandler;
    public UserManagementController(IUserManagementHandler userManagementHandler)
    {
        _userManagementHandler = userManagementHandler;
    }
    
    [HttpPost("register")]
    [ValidateModel]
    public async Task<ActionResult> CreateUser([FromBody] CreateAccountRequest request)
    {
        await _userManagementHandler.CreateAccount(request);
        return Ok();
    }
}
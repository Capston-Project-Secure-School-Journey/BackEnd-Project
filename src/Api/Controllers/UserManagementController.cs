using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Services.UserManagementService;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using Microsoft.AspNetCore.Authentication;
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
    
    [HttpPost]
    [ValidateModel]
    public async Task<ActionResult> CreateUser([FromBody] CreateAccountRequest request)
    {
        await _userManagementHandler.CreateAccount(request);
        return Ok();
    }
}
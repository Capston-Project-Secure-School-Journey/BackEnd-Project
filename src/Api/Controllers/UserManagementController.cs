using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserManagementController: ControllerBase
{
    public UserManagementController(IAuthenticationService authenticationService)
    {
        
    }
}
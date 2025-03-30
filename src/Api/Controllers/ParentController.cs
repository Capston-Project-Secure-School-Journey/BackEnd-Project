using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.DTOs.ChildrenManagement;
using Api.Services.ChildrenManagementService;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("parent")]
public class ParentController : ControllerBase
{
    private readonly IChildrenManagementService _childrenManagementService;

    public ParentController(IChildrenManagementService childrenManagementService)
    {
        _childrenManagementService = childrenManagementService;
    }

    [HttpGet("my-children")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<IEnumerable<ChildDto>> GetMyChildren()
    {
        var userId = this.GetUserId();
        return await _childrenManagementService.GetMyChildren(userId);
    }

    [HttpGet("my-children/{childId}")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<ActionResult<ChildDetailDto>> GetChildById([FromRoute] Guid childId)
    {
        var userId = this.GetUserId();
        return await _childrenManagementService.GetChildById(userId, childId);
    }

    [HttpPost]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    [ValidateModel]
    public async Task<ActionResult> RegisterChild([FromBody] RegisterChildDto request)
    {
        var userId = this.GetUserId();
        await _childrenManagementService.RegisterChild(userId, request);
        return Ok();
    }
    
    [HttpPut("child-pick-up-location")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    [ValidateModel]
    public async Task<ActionResult> UpdateChildPickupLocation([FromBody] UpdateChildPickupLocationDto request)
    {
        var userId = this.GetUserId();
        var message = await _childrenManagementService.UpdateChildPickupLocation(userId, request);
        return Ok(message);
    }
}
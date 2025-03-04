using Api.Attributes;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.DTOs.ChildrenManagement;
using Api.Services.ChildrenManagementService;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("parent")]
public class ParentController: ControllerBase
{
    private readonly IChildrenManagementService _childrenManagementService;
    public ParentController(IChildrenManagementService childrenManagementService)
    {
        _childrenManagementService = childrenManagementService;
    }
    
    [HttpGet("my-children")]
    [Authorize(UserType.Parent)]
    public async Task<IEnumerable<ChildDto>> GetMyChildren()
    {
        var userId = this.GetUserId();
        return await _childrenManagementService.GetMyChildren(userId);
    }
    
    [HttpGet("my-children/{childId}")]
    [Authorize(UserType.Parent)]
    public async Task<ActionResult<ChildDetailDto>> CreateClass([FromRoute] Guid childId)
    {
        var userId = this.GetUserId();
        return await _childrenManagementService.GetChildById(userId, childId);
    }
    
    [HttpPost]
    [Authorize(UserType.Parent)]
    [ValidateModel]
    public async Task<ActionResult> CreateClass([FromBody] RegisterChildDto request)
    {
        var userId = this.GetUserId();
        await _childrenManagementService.RegisterChild(userId, request);
        
        return Ok();
    }
    
}
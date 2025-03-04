using Api.DTOs.ChildrenManagement;

namespace Api.Services.ChildrenManagementService;

public class ChildrenManagementHandler : IChildrenManagementHandler
{
    private readonly IChildrenManagementService _childrenManagementService;

    public ChildrenManagementHandler(IChildrenManagementService childrenManagementService)
    {
        _childrenManagementService = childrenManagementService;
    }

    public async Task<IEnumerable<ChildDto>> GetMyChildren(Guid parentId)
    {
        return await _childrenManagementService.GetMyChildren(parentId);
    }

    public async Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId)
    {
        return await _childrenManagementService.GetChildById(parentId, childId);
    }

    public async Task RegisterChild(Guid parentId, RegisterChildDto dto)
    {
        await _childrenManagementService.RegisterChild(parentId, dto);
    }
}
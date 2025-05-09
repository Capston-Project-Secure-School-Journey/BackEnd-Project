using Api.DTOs.ChildrenManagement;

namespace Api.Services.ChildrenManagementService;

public class ChildrenManagementHandler(IChildrenManagementService childrenManagementService)
    : IChildrenManagementHandler
{
    public async Task<IEnumerable<ChildDto>> GetMyChildren(Guid parentId)
    {
        return await childrenManagementService.GetMyChildren(parentId);
    }

    public async Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId)
    {
        return await childrenManagementService.GetChildById(parentId, childId);
    }

    public async Task RegisterChild(Guid parentId, RegisterChildDto dto)
    {
        await childrenManagementService.RegisterChild(parentId, dto);
    }

    public async Task<string> UpdateChildPickupLocation(Guid parentId, UpdateChildPickupLocationDto dto)
    {
        return await childrenManagementService.UpdateChildPickupLocation(parentId, dto);
    }
}
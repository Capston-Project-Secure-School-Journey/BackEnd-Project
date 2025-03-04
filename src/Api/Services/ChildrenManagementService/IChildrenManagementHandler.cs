using Api.DTOs.ChildrenManagement;

namespace Api.Services.ChildrenManagementService;

public interface IChildrenManagementHandler
{
    Task<IEnumerable<ChildDto>> GetMyChildren(Guid parentId);
    Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId);
    Task RegisterChild(Guid parentId, RegisterChildDto dto);
}
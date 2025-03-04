using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.ClassManagementService;

public interface IClassManagementHandler
{
    Task<Pagination<ClassResponse>> GetClasses(Guid schoolId, GetClassesRequest request);
    Task<ClassResponse> GetClassById(Guid schoolId, Guid id);
    Task<ClassResponse> AddClass(Guid schoolId, CreateClassRequest request);
    Task<ClassResponse> UpdateClass(Guid schoolId, UpdateClassRequest request);
    Task DeleteClass(Guid schoolId, Guid id);
    Task DeleteClass(Guid schoolId, List<Guid> ids);
}
using Api.DTOs.Responses;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;

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
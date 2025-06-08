using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.ClassManagementService;

public interface IClassManagementHandler
{
    Task<Pagination<ClassResponse>> GetClasses(Guid schoolId, GetClassesRequest request);
    Task<ClassDetailResponse> GetClassById(Guid schoolId, Guid id);
    Task<ClassDetailResponse> AddClass(Guid schoolId, CreateClassRequest request);
    Task<ClassDetailResponse> UpdateClass(Guid schoolId, UpdateClassRequest request);
    Task DeleteClass(Guid schoolId, Guid id);
    Task DeleteClass(Guid schoolId, List<Guid> ids);
    Task<MemoryStream> GetTemplateExcelFile();
    Task ImportClassesFromExcelFile(Guid schoolId, IFormFile file);
}
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.TeacherManagementService;

public interface ITeacherManagementHandler
{
    Task<Pagination<TeacherResponse>> GetTeachers(Guid schoolId, GetTeacherRequest request);
    Task<TeacherDetailResponse> GetTeacherById(Guid schoolId, Guid id);
    Task<TeacherDetailResponse> AddTeacher(Guid schoolId, CreateTeacherRequest request);
    Task<TeacherDetailResponse> UpdateTeacher(Guid schoolId, UpdateTeacherRequest request);
    Task DeleteTeacher(Guid schoolId, Guid id);
    Task DeleteTeacher(Guid schoolId, List<Guid> ids);
    Task<string> UploadAvatar(Guid schoolId, Guid teacherId, IFormFile file);
    Task<MemoryStream> GetTemplateExcelFile();
    Task ImportTeachersFromExcelFile(Guid schoolId, IFormFile file);
}
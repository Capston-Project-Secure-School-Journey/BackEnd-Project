using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.StudentManagementService;

public interface IStudentManagementHandler
{
    Task<Pagination<StudentResponse>> GetStudents(Guid schoolId, GetStudentRequest request);
    Task<StudentDetailResponse> GetStudentById(Guid schoolId, Guid id);
    Task<IEnumerable<StudentResponse>> GetMyChildren(Guid parentId);
    Task<StudentDetailResponse> AddStudent(Guid schoolId, CreateStudentRequest request);
    Task<StudentDetailResponse> UpdateStudent(Guid schoolId, UpdateStudentRequest request);
    Task DeleteStudent(Guid schoolId, Guid id);
    Task DeleteStudent(Guid schoolId, List<Guid> ids);
    Task<string> UploadAvatar(Guid schoolId, Guid studentId, IFormFile file);
    Task<MemoryStream> GetTemplateExcelFile();
    Task ImportStudentsFromExcelFile(Guid schoolId, IFormFile file);
}
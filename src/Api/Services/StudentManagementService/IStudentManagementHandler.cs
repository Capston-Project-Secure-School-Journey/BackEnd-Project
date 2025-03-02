using Api.DTOs.Responses;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;

namespace Api.Services.StudentManagementService;

public interface IStudentManagementHandler
{
    Task<Pagination<StudentResponse>> GetStudents(Guid schoolId, GetStudentRequest request);
    Task<StudentResponse> GetStudentById(Guid schoolId, Guid id);
    Task<IEnumerable<StudentResponse>> GetMyChildren(Guid parentId);
    Task<StudentResponse> AddStudent(Guid schoolId, CreateStudentRequest request);
    Task<StudentResponse> UpdateStudent(Guid schoolId, UpdateStudentRequest request);
    Task DeleteStudent(Guid schoolId, Guid id);
    Task DeleteStudent(Guid schoolId, List<Guid> ids);
}
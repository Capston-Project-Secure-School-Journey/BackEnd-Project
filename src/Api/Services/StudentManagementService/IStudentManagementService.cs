using Api.Domain.Models;
using Api.DTOs.StudentManagement;

namespace Api.Services.StudentManagementService;

public interface IStudentManagementService
{
    Task<IEnumerable<Student>> GetStudents(Guid schoolId);
    Task<IEnumerable<Student>> GetStudentsByFilter(Guid schoolId, string? name, Guid? classId);
    Task<IQueryable<Student>> GetStudentsByFilterQueryAble(Guid schoolId, string? name, Guid? classId);
    Task<Student> GetStudentById(Guid id);
    Task<Student> AddStudent(CreateStudentDto request);
    Task<Student> UpdateStudent(UpdateStudentDto request);
    Task DeleteStudent(Guid id);
    Task DeleteStudent(List<Guid> ids); 
    Task CheckExistStudent(Guid schoolId, Guid studentId);
    Task IsOwnerOfStudent(Guid schoolId, Guid studentId);
    Task<string> UploadAvatar(Guid studentId, IFormFile file);
}
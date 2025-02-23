using Api.Domain.Models;
using Api.DTOs.TeacherManagement;

namespace Api.Services.TeacherManagementService;

public interface ITeacherManagementService
{
    Task<IEnumerable<Teacher>> GetTeachers(Guid schoolId);
    Task<IEnumerable<Teacher>> GetTeachersByFilter(Guid schoolId, string? name, string? email, string? phoneNumber);
    Task<IQueryable<Teacher>> GetTeachersByFilterQueryAble(Guid schoolId, string? name, string? email, string? phoneNumber);
    Task<Teacher> GetTeacherById(Guid id);
    Task<Teacher> AddTeacher(CreateTeacherDto request);
    Task<Teacher> UpdateTeacher(UpdateTeacherDto request);
    Task DeleteTeacher(Guid id);
    Task DeleteTeacher(List<Guid> ids); 
    Task CheckExistTeacher(Guid schoolId, Guid teacherId);
    Task IsOwnerOfTeacher(Guid schoolId, Guid teacherId);
}
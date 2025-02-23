using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.ClassManagement;

namespace Api.Services.ClassManagementService;

public interface IClassManagementService
{
    Task<IEnumerable<Class>> GetClasses(Guid schoolId);
    Task<IEnumerable<Class>> GetClassesByFilter(Guid schoolId, string? className, Grade? grade);
    Task<IQueryable<Class>> GetClassesQueryAbleByFilter(Guid schoolId, string? className, Grade? grade);
    Task<Class> GetClassById(Guid id);
    Task<Class> AddClass(CreateClassDto request);
    Task<Class> UpdateClass(UpdateClassDto request);
    Task DeleteClass(Guid id);
    Task DeleteClass(List<Guid> ids);
    Task IsOwnerOfClass(Guid schoolId, Guid classId);
}
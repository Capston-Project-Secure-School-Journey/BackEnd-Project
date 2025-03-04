using Api.Domain.Models;
using Api.DTOs.UserManagement;

namespace Api.Services.UserManagementService;

public interface IUserManagement
{
    Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request);
    Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword);
    Task<User> CreateUser(CreateUserDto request);
    Task DeleteSchoolAdmin(Guid schoolId);
    Task DeleteSchoolAdmin(List<Guid> schoolIds);
    Task<IEnumerable<SchoolPerson>> GetListOfSchoolAdmins();
}
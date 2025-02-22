using Api.Domain.Models;
using Api.DTOs.UserManagement;

namespace Api.IOC.Services.UserManagementService;

public interface IUserManagement
{
    Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request);
    Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword);
    Task DeleteSchoolAdmin(Guid schoolId);
    Task DeleteSchoolAdmin(List<Guid> schoolIds);
    Task<IEnumerable<SchoolPerson>> GetListOfSchoolAdmins();
}
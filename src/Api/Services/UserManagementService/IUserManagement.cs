using Api.Domain.Models;
using Api.DTOs.UserManagement;

namespace Api.IOC.Services.UserManagementService;

public interface IUserManagement
{
    Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request);
    Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword);
    Task<bool> DeleteSchoolAdmin(Guid userId);
    Task<bool> DeleteSchoolAdmin(List<Guid> userId);
    Task<IEnumerable<SchoolPerson>> GetListOfSchoolAdmins();
}
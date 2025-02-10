using System.ComponentModel;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UserManagement;

namespace Api.IOC.Services.UserManagementService;

public class UserManagement: IUserManagement
{
    private readonly Context _context;
    public UserManagement(Context context)
    {
        _context = context;
    }

    public Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request)
    {
        throw new NotImplementedException();
    }

    public Task<SchoolPerson> UpdateSchoolAdmin(UpdateSchoolAdminDto request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteSchoolAdmin(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteSchoolAdmin(List<Guid> userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SchoolPerson>> GetListOfSchoolAdmins()
    {
        throw new NotImplementedException();
    }
}
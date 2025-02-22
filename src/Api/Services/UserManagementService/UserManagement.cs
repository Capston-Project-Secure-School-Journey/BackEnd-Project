using System.ComponentModel;
using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace Api.IOC.Services.UserManagementService;

public class UserManagement: IUserManagement
{
    private readonly Context _context;
    public UserManagement(Context context)
    {
        _context = context;
    }

    public async Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request)
    {
        if (_context.SchoolPersons.Any(x => x.SchoolId == request.SchoolId && x.UserType == UserType.SchoolAdmin))
            throw new BadRequestException("Đã tồn tại tài khoản của người quản trị trường học");
        
        var user = new SchoolPerson
        {
            UserName = request.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserType = UserType.SchoolAdmin,
            AccountStatus = AccountStatus.Verified,
            SchoolId = request.SchoolId
        };

        await _context.SchoolPersons.AddAsync(user);
        _context.Entry(user).State = EntityState.Added;
        
        await _context.SaveChangesAsync();
        
        return user;
    }

    public async Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword)
    {
        var schoolAdmin = await _context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (schoolAdmin == null)
            throw new NotFoundException("Không tồn tại tài khoản");

        schoolAdmin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _context.Entry(schoolAdmin).State = EntityState.Modified;
        await _context.SaveChangesAsync();
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
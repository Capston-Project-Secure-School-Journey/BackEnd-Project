using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UserManagementService;

public class UserManagement : IUserManagement
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

        if (_context.Users.Any(x => x.UserName == request.UserName))
            throw new BadRequestException("Tên đăng nhập đã tồn tại");

        var user = new SchoolPerson
        {
            UserName = request.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserType = UserType.SchoolAdmin,
            AccountStatus = AccountStatus.Verified,
            VerificationMethod = VerificationMethod.Email,
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

    public async Task<User> CreateUser(CreateUserDto request)
    {
        if (await _context.Users.AnyAsync(x => x.UserName == request.UserName))
            throw new BadRequestException("Đã tồn tại tài khoản.");

        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
            throw new BadRequestException("Email và số điện thoại đều trống. Vui lòng điền ít nhất 1.");

        if (!string.IsNullOrEmpty(request.Email))
            if (_context.Users.Any(x => x.Email == request.Email))
                throw new BadRequestException("Email đã được đăng kí.");

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            if (_context.Users.Any(x => x.PhoneNumber == request.PhoneNumber))
                throw new BadRequestException("Số điện thoại đã được đăng kí.");


        if (request.UserType != UserType.Driver && request.UserType != UserType.Parent)
            throw new BadRequestException("Không thể đăng kí tài khoản.");


        User user;
        if (request.UserType == UserType.Driver)
        {
            var driver = new Driver()
            {
                UserName = request.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserType = request.UserType,
                AccountStatus = AccountStatus.New,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Gender = request.Gender
            };
            _context.Drivers.Add(driver);
            _context.Entry(driver).State = EntityState.Added;
            user = driver;
        }
        else
        {
            var parent = new Parent()
            {
                UserName = request.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserType = request.UserType,
                AccountStatus = AccountStatus.New,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Gender = request.Gender
            };

            _context.Parents.Add(parent);
            _context.Entry(parent).State = EntityState.Added;
            user = parent;
        }


        await _context.SaveChangesAsync();
        return user;
    }


    public Task<SchoolPerson> UpdateSchoolAdmin(UpdateSchoolAdminDto request)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteSchoolAdmin(Guid schoolId)
    {
        var schoolAdmin = await _context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (schoolAdmin == null)
            throw new NotFoundException("Không tồn tại tài khoản");


        schoolAdmin.AccountStatus = AccountStatus.Deactive;

        _context.Entry(schoolAdmin).State = EntityState.Deleted;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteSchoolAdmin(List<Guid> schoolIds)
    {
        var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var i in schoolIds) await DeleteSchoolAdmin(i);

            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public Task<IEnumerable<SchoolPerson>> GetListOfSchoolAdmins()
    {
        throw new NotImplementedException();
    }
}
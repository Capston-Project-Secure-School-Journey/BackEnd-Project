using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UserManagementService;

public class UserManagement(Context context) : IUserManagement
{
    public async Task<SchoolPerson> CreateSchoolAdmin(CreateSchoolAdminDto request)
    {
        if (await context.SchoolPersons.AnyAsync(x =>
                x.SchoolId == request.SchoolId && x.UserType == UserType.SchoolAdmin))
            throw new BadRequestException(ErrorMessages.AccountExists);

        if (await context.Users.AnyAsync(x => x.UserName == request.UserName))
            throw new BadRequestException(ErrorMessages.UsernameExists);

        var user = new SchoolPerson
        {
            UserName = request.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserType = UserType.SchoolAdmin,
            AccountStatus = AccountStatus.Verified,
            VerificationMethod = VerificationMethod.Email,
            SchoolId = request.SchoolId
        };

        await context.SchoolPersons.AddAsync(user);
        context.Entry(user).State = EntityState.Added;

        await context.SaveChangesAsync();

        return user;
    }

    public async Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword)
    {
        var schoolAdmin = await context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (schoolAdmin == null)
            throw new NotFoundException(ErrorMessages.AccountNotExist);

        schoolAdmin.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        context.SchoolPersons.Update(schoolAdmin);
        await context.SaveChangesAsync();
    }

    public async Task<User> CreateUser(CreateUserDto request)
    {
        if (await context.Users.AnyAsync(x => x.UserName == request.UserName))
            throw new BadRequestException(ErrorMessages.AccountExists);

        if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
            throw new BadRequestException(ErrorMessages.EmailOrPhoneRequired);

        if (!string.IsNullOrEmpty(request.Email) &&
            await context.Users.AnyAsync(x => x.Email == request.Email)
           )
            throw new BadRequestException(ErrorMessages.EmailExists);

        if (!string.IsNullOrEmpty(request.PhoneNumber) &&
            await context.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber)
           )
            throw new BadRequestException(ErrorMessages.PhoneExists);


        if (request.UserType != UserType.Driver && request.UserType != UserType.Parent)
            throw new BadRequestException(ErrorMessages.CannotRegisterAccount);


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
            context.Drivers.Add(driver);
            context.Entry(driver).State = EntityState.Added;
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

            context.Parents.Add(parent);
            context.Entry(parent).State = EntityState.Added;
            user = parent;
        }


        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteSchoolAdmin(Guid schoolId)
    {
        var schoolAdmin = await context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (schoolAdmin == null)
            throw new NotFoundException(ErrorMessages.AccountNotExist);


        schoolAdmin.AccountStatus = AccountStatus.DeActive;

        context.SchoolPersons.Remove(schoolAdmin);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSchoolAdmin(List<Guid> schoolIds)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var i in schoolIds) await DeleteSchoolAdmin(i);

            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task<User> GetSchoolAdmin(Guid schoolId)
    {
        var schoolAdmin = await context.SchoolPersons
            .FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.UserType == UserType.SchoolAdmin);

        if (schoolAdmin == null)
            throw new NotFoundException(ErrorMessages.AccountNotExist);

        return schoolAdmin;
    }
}
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.TeacherManagement;
using Api.DTOs.UploadFileService;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.TeacherManagementService;

public class TeacherManagementService(
    Context context,
    IFileUploadService uploadService,
    ILogger<TeacherManagementService> logger) : ITeacherManagementService
{
    public async Task<IEnumerable<Teacher>> GetTeachers(Guid schoolId)
    {
        return await context.Teachers
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetTeachersByFilter(Guid schoolId, string? name, string? email,
        string? phoneNumber)
    {
        var query = await GetTeachersByFilterQueryAble(schoolId, name, email, phoneNumber);

        return await query.ToListAsync();
    }

    public Task<IQueryable<Teacher>> GetTeachersByFilterQueryAble(Guid schoolId, string? name, string? email,
        string? phoneNumber)
    {
        var query = context.Teachers.AsQueryable()
            .AsNoTracking();

        query = query.Where(x => x.SchoolId == schoolId);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(t => EF.Functions.Like(t.FullName, name + "%"));
        if (!string.IsNullOrEmpty(email))
            query = query.Where(t => EF.Functions.Like(t.Email, email + "%"));
        if (!string.IsNullOrEmpty(phoneNumber))
            query = query.Where(t => EF.Functions.Like(t.PhoneNumber, phoneNumber + "%"));

        return Task.FromResult(query);
    }

    public async Task<Teacher> GetTeacherById(Guid id)
    {
        var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            throw new NotFoundException(ErrorMessages.TeacherNotFound);
        return teacher;
    }

    public async Task<Teacher> AddTeacher(CreateTeacherDto request)
    {
        var teacher = new Teacher
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            SchoolId = request.SchoolId
        };

        context.Teachers.Add(teacher);
        context.Entry(teacher).State = EntityState.Added;
        await context.SaveChangesAsync();

        return teacher;
    }

    public async Task<Teacher> UpdateTeacher(UpdateTeacherDto request)
    {
        var teacher = await GetTeacherById(request.Id);
        teacher.FirstName = request.FirstName;
        teacher.LastName = request.LastName;
        teacher.PhoneNumber = request.PhoneNumber;
        teacher.Email = request.Email;
        teacher.DateOfBirth = request.DateOfBirth;
        teacher.Gender = request.Gender;

        context.Teachers.Update(teacher);
        await context.SaveChangesAsync();

        return teacher;
    }

    public async Task DeleteTeacher(Guid id)
    {
        var teacher = await GetTeacherById(id);
        var managedClasses = (await context.Classes
                .Where(c => c.SchoolId == teacher.SchoolId)
                .ToListAsync())
            .Where(c => c.ManagedTeachers.Any(t => t.ManagedTeacherId == id));

        foreach (var i in managedClasses)
        {
            i.ManagedTeachers.Remove(i.ManagedTeachers.First(t => t.ManagedTeacherId == id));
            context.Classes.Update(i);
        }

        context.Teachers.Remove(teacher);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTeacher(List<Guid> ids)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var id in ids)
                await DeleteTeacher(id);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
        }
    }

    public async Task CheckExistTeacher(Guid schoolId, Guid teacherId)
    {
        if (!await context.Teachers.AnyAsync(t => t.SchoolId == schoolId && t.Id == teacherId))
            throw new NotFoundException(ErrorMessages.TeacherNotFound);
    }

    public async Task IsOwnerOfTeacher(Guid schoolId, Guid teacherId)
    {
        if (!await context.Teachers.AnyAsync(t => t.SchoolId == schoolId && t.Id == teacherId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    public async Task<string> UploadAvatar(Guid teacherId, IFormFile file)
    {
        var teacher = await GetTeacherById(teacherId);
        await uploadService.BeginTransactionAsync();
        UploadFileResponse uploadResponse;
        try
        {
            if (teacher.AvatarKey != null)
                await uploadService.DeleteFileManagementAsync(teacher.AvatarKey.Value);
            uploadResponse = await uploadService.UploadFileAsync(file, "avatar/teachers");

            teacher.AvatarKey = uploadResponse.Key;
            context.Teachers.Update(teacher);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await uploadService.RollBackAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "UploadFileService.RollBackAsync");
                }
            });
            throw;
        }

        return uploadResponse.S3Url;
    }
}
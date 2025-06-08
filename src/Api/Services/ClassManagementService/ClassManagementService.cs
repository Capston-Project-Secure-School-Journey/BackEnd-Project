using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.Services.SchoolManagement;
using Api.Services.TeacherManagementService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ClassManagementService;

public class ClassManagementService(
    Context context,
    ISchoolManagement schoolManagement,
    ITeacherManagementService teacherManagement)
    : IClassManagementService
{
    public async Task<IEnumerable<Class>> GetClasses(Guid schoolId)
    {
        return await context.Classes
            .Where(cl => cl.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesByFilter(Guid schoolId, string? className, Grade? grade)
    {
        var query = await GetClassesQueryAbleByFilter(schoolId, className, grade);

        return await query.ToListAsync();
    }

    public Task<IQueryable<Class>> GetClassesQueryAbleByFilter(Guid schoolId, string? className, Grade? grade)
    {
        var query = context.Classes.AsQueryable()
            .AsNoTracking()
            .Where(cl => cl.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(className))
            query = query.Where(cl => EF.Functions.Like(cl.ClassName, className + "%"));

        if (grade.HasValue) query = query.Where(cl => cl.Grade == grade);

        return Task.FromResult(query);
    }

    public async Task<Class> GetClassById(Guid id)
    {
        var cl = await context.Classes.FirstOrDefaultAsync(x => x.Id == id);

        if (cl == null) throw new NotFoundException(ErrorMessages.ClassNotFound);

        return cl;
    }

    public async Task<Class> AddClass(CreateClassDto request)
    {
        var school = await schoolManagement.GetSchool(request.SchoolId);

        ValidateGrade(school.SchoolType, request.Grade);
        foreach (var i in request.ManagedTeachers)
            await teacherManagement.CheckExistTeacher(request.SchoolId, i.ManagedTeacherId);

        await CheckExistClassName(request.SchoolId, request.ClassName);

        var cl = new Class()
        {
            SchoolId = request.SchoolId,
            ClassName = request.ClassName,
            Grade = request.Grade,
            ManagedTeachers = request.ManagedTeachers,
            NumberOfStudent = 0
        };

        context.Classes.Add(cl);
        context.Entry(cl).State = EntityState.Added;
        await context.SaveChangesAsync();

        return cl;
    }

    public async Task<Class> UpdateClass(UpdateClassDto request)
    {
        var school = await schoolManagement.GetSchool(request.SchoolId);
        var cl = await GetClassById(request.Id);

        foreach (var i in request.ManagedTeachers)
            await teacherManagement.CheckExistTeacher(request.SchoolId, i.ManagedTeacherId);

        ValidateGrade(school.SchoolType, request.Grade);

        if (cl.ClassName != request.ClassName)
            await CheckExistClassName(request.SchoolId, request.ClassName);

        cl.ClassName = request.ClassName;
        cl.Grade = request.Grade;
        cl.ManagedTeachers = request.ManagedTeachers;

        context.Classes.Update(cl);
        await context.SaveChangesAsync();

        return cl;
    }

    public async Task DeleteClass(Guid id)
    {
        var cl = await GetClassById(id);

        await context.Entry(cl)
            .Collection<Student>(c => c.Students)
            .LoadAsync();

        // Delete students
        context.Students.RemoveRange(cl.Students);

        context.Classes.Remove(cl);
        await context.SaveChangesAsync();
    }

    public async Task DeleteClass(List<Guid> ids)
    {
        foreach (var id in ids) await DeleteClass(id);
    }

    public async Task IsOwnerOfClass(Guid schoolId, Guid classId)
    {
        if (!await context.Classes.AnyAsync(t => t.SchoolId == schoolId && t.Id == classId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    public async Task ImportClassesFromExcel(Guid schoolId, List<Class> classes)
    {
        var index = 1;
        try
        {
            var school = await schoolManagement.GetSchool(schoolId);
            foreach (var @class in classes)
            {
                await CheckExistClassName(schoolId, @class.ClassName);

                ValidateGrade(school.SchoolType, @class.Grade);

                foreach (var i in @class.ManagedTeachers)
                    await teacherManagement.CheckExistTeacher(schoolId, i.ManagedTeacherId);

                @class.NumberOfStudent = 0;
                @class.Id = Guid.NewGuid();
                @class.SchoolId = schoolId;

                index++;
            }

            await context.Classes.AddRangeAsync(classes);
            await context.SaveChangesAsync();
        }
        catch (Exception e) when (e is BadRequestException or NotFoundException or ValidationException)
        {
            throw new BadRequestException($"Lỗi tại dòng: {index}, Chi tiết lỗi: {e.Message}");
        }
    }

    private async Task CheckExistClassName(Guid schoolId, string className)
    {
        if (await context.Classes.AnyAsync(cl => cl.ClassName == className && cl.SchoolId == schoolId))
            throw new BadRequestException(ErrorMessages.DuplicateClassName);
    }

    private static void ValidateGrade(SchoolType schoolType, Grade grade)
    {
        switch (schoolType)
        {
            case SchoolType.Preschool:
                if (grade is not Grade.Daycare and not Grade.JuniorKindergarten and not Grade.SeniorKindergarten)
                    throw new ValidationException(ErrorMessages.InvalidPreschoolClass);
                break;

            case SchoolType.PrimarySchool:
                if (grade is not (Grade.Grade1 or Grade.Grade2 or Grade.Grade3 or Grade.Grade4 or Grade.Grade5))
                    throw new ValidationException(ErrorMessages.InvalidPrimaryClass);
                break;

            case SchoolType.MiddleSchool:
                if (grade is not (Grade.Grade6 or Grade.Grade7 or Grade.Grade8 or Grade.Grade9))
                    throw new ValidationException(ErrorMessages.InvalidSecondaryClass);
                break;

            case SchoolType.HighSchool:
                if (grade is not (Grade.Grade10 or Grade.Grade11 or Grade.Grade12))
                    throw new ValidationException(ErrorMessages.InvalidHighSchoolClass);
                break;

            default:
                throw new ValidationException(ErrorMessages.InvalidSchoolType);
        }
    }
}
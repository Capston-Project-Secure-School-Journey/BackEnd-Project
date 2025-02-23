using System.ComponentModel.DataAnnotations;
using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.Services.SchoolManagement;
using Api.Services.TeacherManagementService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ClassManagementService;

public class ClassManagementService : IClassManagementService
{
    private readonly Context _context;
    private readonly ISchoolManagement _schoolManagement;
    private readonly ITeacherManagementService _teacherManagement;

    public ClassManagementService(Context context,
        ISchoolManagement schoolManagement,
        ITeacherManagementService teacherManagement)
    {
        _context = context;
        _schoolManagement = schoolManagement;
        _teacherManagement = teacherManagement;
    }

    public async Task<IEnumerable<Class>> GetClasses(Guid schoolId)
    {
        return await _context.Classes
            .Where(cl => cl.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Class>> GetClassesByFilter(Guid schoolId, string? className, Grade? grade)
    {
        var query = await GetClassesQueryAbleByFilter(schoolId, className, grade);

        return await query.ToListAsync();
    }

    public async Task<IQueryable<Class>> GetClassesQueryAbleByFilter(Guid schoolId, string? className, Grade? grade)
    {
        var query = _context.Classes.AsQueryable()
            .AsNoTracking()
            .Where(cl => cl.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(className))
        {
            query = query.Where(cl => cl.ClassName.ToLower().Contains(className.ToLower()));
        }

        if (grade.HasValue)
        {
            query = query.Where(cl => cl.Grade == grade);
        }

        return query;
    }

    public async Task<Class> GetClassById(Guid id)
    {
        var cl = await _context.Classes.FirstOrDefaultAsync(x => x.Id == id);

        if (cl == null)
        {
            throw new NotFoundException("Lớp học không tồn tại");
        }

        return cl;
    }

    public async Task<Class> AddClass(CreateClassDto request)
    {
        var school = await _schoolManagement.GetSchool(request.SchoolId);

        ValidateGrade(school.SchoolType, request.Grade);
        foreach (var i in request.ManagedTeachers)
        {
            await _teacherManagement.CheckExistTeacher(request.SchoolId, i.ManagedTeacherId);
        }

        await CheckExistClassName(request.SchoolId, request.ClassName);

        var cl = new Class()
        {
            SchoolId = request.SchoolId,
            ClassName = request.ClassName,
            Grade = request.Grade,
            ManagedTeachers = request.ManagedTeachers,
            NumberOfStudent = 0
        };

        _context.Classes.Add(cl);
        _context.Entry(cl).State = EntityState.Added;
        await _context.SaveChangesAsync();

        return cl;
    }

    public async Task<Class> UpdateClass(UpdateClassDto request)
    {
        var school = await _schoolManagement.GetSchool(request.SchoolId);
        var cl = await GetClassById(request.Id);

        foreach (var i in request.ManagedTeachers)
        {
            await _teacherManagement.CheckExistTeacher(request.SchoolId, i.ManagedTeacherId);
        }
        
        ValidateGrade(school.SchoolType, request.Grade);
        
        if (cl.ClassName != request.ClassName)
            await CheckExistClassName(request.SchoolId, request.ClassName);

        cl.ClassName = request.ClassName;
        cl.Grade = request.Grade;
        cl.ManagedTeachers = request.ManagedTeachers;

        _context.Entry(cl).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return cl;
    }

    public async Task DeleteClass(Guid id)
    {
        try
        {
            var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                await DeleteClassNoTransaction(id);

                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            throw new DatabaseException("Không thể xóa dữ liệu");
        }
    }

    public async Task DeleteClass(List<Guid> ids)
    {
        try
        {
            var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var id in ids)
                {
                    await DeleteClassNoTransaction(id);
                }

                await trans.CommitAsync();
            }
            catch (Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }
        catch (Exception)
        {
            throw new DatabaseException("Không thể xóa dữ liệu");
        }
    }

    public async Task IsOwnerOfClass(Guid schoolId, Guid classId)
    {
        if (!await _context.Classes.AnyAsync(t => t.SchoolId == schoolId && t.Id == classId))
        {
            throw new ForbiddenException("Bạn không có quyền truy cập");
        }
    }

    private async Task CheckExistClassName(Guid schoolId, string className)
    {
        if (await _context.Classes.AnyAsync(cl => cl.ClassName == className && cl.SchoolId == schoolId))
        {
            throw new BadRequestException("Tên lớp bị trùng");
        }
    }

    private async Task DeleteClassNoTransaction(Guid id)
    {
        var cl = await GetClassById(id);
        _context.Entry(cl).State = EntityState.Deleted;

        await _context.Entry(cl)
            .Collection<Student>(c => c.Students)
            .LoadAsync();

        // Delete students
        foreach (var clStudent in cl.Students)
        {
            _context.Entry(clStudent).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync();
    }

    private void ValidateGrade(SchoolType schoolType, Grade grade)
    {
        switch (schoolType)
        {
            case SchoolType.Preschool:
                if (grade is not Grade.Daycare and not Grade.JuniorKindergarten and not Grade.SeniorKindergarten)
                    throw new ValidationException("Lớp không hợp lệ cho trường Mầm non.");
                break;

            case SchoolType.PrimarySchool:
                if (grade is not (Grade.Grade1 or Grade.Grade2 or Grade.Grade3 or Grade.Grade4 or Grade.Grade5))
                    throw new ValidationException("Lớp không hợp lệ cho trường Tiểu học.");
                break;

            case SchoolType.MiddleSchool:
                if (grade is not (Grade.Grade6 or Grade.Grade7 or Grade.Grade8 or Grade.Grade9))
                    throw new ValidationException("Lớp không hợp lệ cho trường Trung học cơ sở.");
                break;

            case SchoolType.HighSchool:
                if (grade is not (Grade.Grade10 or Grade.Grade11 or Grade.Grade12))
                    throw new ValidationException("Lớp không hợp lệ cho trường Trung học phổ thông.");
                break;

            default:
                throw new ValidationException("Loại trường không hợp lệ.");
        }
    }
}
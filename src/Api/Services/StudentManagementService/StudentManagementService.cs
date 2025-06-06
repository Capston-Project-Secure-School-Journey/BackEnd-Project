using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.StudentManagement;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.StudentManagementService;

public class StudentManagementService(
    Context context,
    IClassManagementService classManagementService,
    IFileUploadService uploadFileService,
    IQrCodeGenerator qrCodeGenerator,
    ILogger<StudentManagementService> logger)
    : IStudentManagementService
{
    public async Task<IEnumerable<Student>> GetStudents(Guid schoolId)
    {
        return await context.Students
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByFilter(Guid schoolId, Guid? studentId, string? name,
        Guid? classId,
        string? className = null)
    {
        var query = await GetStudentsByFilterQueryAble(schoolId, studentId, name, classId, className);

        return await query.ToListAsync();
    }

    public Task<IQueryable<Student>> GetStudentsByFilterQueryAble(Guid schoolId,
        Guid? studentId, string? name, Guid? classId,
        string? className = null)
    {
        var query = context.Students
            .Include(s => s.Class)
            .AsQueryable()
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId);

        if (studentId.HasValue)
            query = query.Where(st => st.Id == studentId);
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(st => EF.Functions.Like(st.FullName, name + "%"));
        if (classId.HasValue)
            query = query.Where(st => st.ClassId == classId);
        if (!string.IsNullOrWhiteSpace(className))
            query = query.Where(st => EF.Functions.Like(st.Class.ClassName, className + "%"));

        return Task.FromResult(query);
    }

    public async Task<Student> GetStudentById(Guid id)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null)
            throw new NotFoundException(ErrorMessages.StudentNotExist);

        return student;
    }

    public async Task<Student> AddStudent(CreateStudentDto request)
    {
        var trans = await context.Database.BeginTransactionAsync();
        await uploadFileService.BeginTransactionAsync();
        try
        {
            var cl = await classManagementService.GetClassById(request.ClassId);
            if (cl.SchoolId != request.SchoolId)
                throw new BadRequestException(ErrorMessages.ClassNotExist);

            var st = new Student()
            {
                SchoolId = request.SchoolId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                ClassId = request.ClassId
            };

            cl.NumberOfStudent += 1;

            context.Students.Add(st);
            context.Classes.Update(cl);
            await context.SaveChangesAsync();
            var hash = HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(st.Id));
            var stream = qrCodeGenerator.GenerateQrCodeStream(hash);
            var uploadRe = await uploadFileService.UploadStreamAsync(stream,
                st.Id.ToString() + ".png", "image/png", "student_qr_images");
            st.QrImageKey = uploadRe.Key;

            await context.SaveChangesAsync();
            await trans.CommitAsync();

            return st;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            uploadFileService
                .RollBackAsync()
                .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
            throw;
        }
    }

    public async Task<Student> UpdateStudent(UpdateStudentDto request)
    {
        var cl = await classManagementService.GetClassById(request.ClassId);
        if (cl.SchoolId != request.SchoolId)
            throw new BadRequestException(ErrorMessages.ClassNotExist);

        var st = await GetStudentById(request.Id);
        var oldClass = await classManagementService.GetClassById(st.ClassId);
        oldClass.NumberOfStudent -= 1;
        cl.NumberOfStudent += 1;

        st.FirstName = request.FirstName;
        st.LastName = request.LastName;
        st.DateOfBirth = request.DateOfBirth;
        st.Gender = request.Gender;
        st.ClassId = request.ClassId;

        context.Students.Update(st);
        context.Classes.Update(cl);
        context.Classes.Update(oldClass);
        await context.SaveChangesAsync();
        return st;
    }

    public async Task DeleteStudent(Guid id)
    {
        var student = await GetStudentById(id);
        await context.Entry(student).Reference(x => x.Class).LoadAsync();
        student.Class.NumberOfStudent -= 1;
        context.Students.Remove(student);
        context.Classes.Update(student.Class);

        await context.SaveChangesAsync();
    }

    public async Task DeleteStudent(List<Guid> ids)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var id in ids)
                await DeleteStudent(id);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
        }
    }

    public async Task CheckExistStudent(Guid schoolId, Guid studentId)
    {
        if (!await context.Students.AnyAsync(s => s.SchoolId == schoolId && s.Id == studentId))
            throw new NotFoundException(ErrorMessages.StudentNotExist);
    }

    public async Task IsOwnerOfStudent(Guid schoolId, Guid studentId)
    {
        if (!await context.Students.AnyAsync(s => s.SchoolId == schoolId && s.Id == studentId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    public async Task<string> UploadAvatar(Guid studentId, IFormFile file)
    {
        var student = await GetStudentById(studentId);
        await uploadFileService.BeginTransactionAsync();
        UploadFileResponse uploadResponse;
        try
        {
            if (student.AvatarKey != null)
                await uploadFileService.DeleteFileManagementAsync(student.AvatarKey.Value);
            uploadResponse = await uploadFileService.UploadFileAsync(file, "avatar/students");

            student.AvatarKey = uploadResponse.Key;
            context.Students.Update(student);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            uploadFileService
                .RollBackAsync()
                .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
            throw;
        }

        return uploadResponse.S3Url;
    }
}
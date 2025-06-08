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
    ILogger<StudentManagementService> logger,
    GoogleMapsService googleMapsService)
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
        string? className)
    {
        var query = await GetStudentsByFilterQueryAble(schoolId, studentId, name, classId, className);

        return await query.ToListAsync();
    }

    public Task<IQueryable<Student>> GetStudentsByFilterQueryAble(Guid schoolId,
        Guid? studentId, string? name, Guid? classId,
        string? className)
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
        var stream = qrCodeGenerator.GenerateQrCodeStream(GetStudentHash(st.Id));
        var uploadRe = await uploadFileService.UploadStreamAsync(stream,
            st.Id.ToString() + ".png", "image/png", "student_qr_images");
        st.QrImageKey = uploadRe.Key;

        await context.SaveChangesAsync();

        return st;
    }

    public async Task ImportStudentsFromExcel(Guid schoolId, List<Student> students)
    {
        var cache = new Dictionary<Guid, bool>();
        var index = 0;
        foreach (var classId in students.Select(st => st.ClassId))
        {
            index++;
            if (cache.TryGetValue(classId, out var _))
                continue;
            var exist = await context.Classes
                .Where(c => c.Id == classId)
                .Select(c => c.SchoolId)
                .FirstOrDefaultAsync();
            
            if (exist == Guid.Empty || exist != schoolId)
                throw new BadRequestException($"Lỗi tại dòng {index}." +
                                              ErrorMessages.ClassNotExistWithClassId.Replace("{0}",
                                                  classId.ToString()));
            cache.Add(classId, true);
        }

        index = 0;
        foreach (var student in students)
        {
            index++;
            student.Id = Guid.NewGuid();
            student.SchoolId = schoolId;

            var stream = qrCodeGenerator.GenerateQrCodeStream(GetStudentHash(student.Id));
            var uploadRe = await uploadFileService.UploadStreamAsync(stream,
                student.Id + ".png", "image/png", "student_qr_images");
            student.QrImageKey = uploadRe.Key;

            if (!await googleMapsService.IsCarAccessibleAddressAsync(student.PickUpLocation))
                throw new BadRequestException($"Lỗi tại dòng {index}." +
                                              $"Địa chỉ {student.PickUpLocation} ôtô không thể đi vào.");

            var locationAddress = await googleMapsService.GetLatLngFromAddressAsync(student.PickUpLocation);
            student.PickUpLat = locationAddress.lat;
            student.PickUpLng = locationAddress.lng;
        }

        await context.Students.AddRangeAsync(students);
        await context.SaveChangesAsync();
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
        foreach (var id in ids)
            await DeleteStudent(id);
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

    public static string GetStudentHash(Guid studentId)
    {
        var hash = HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(studentId));
        return hash;
    }
}
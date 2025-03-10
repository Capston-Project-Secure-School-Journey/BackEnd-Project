using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.StudentManagement;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.StudentManagementService;

public class StudentManagementService : IStudentManagementService
{
    private readonly Context _context;
    private readonly IClassManagementService _classManagementService;
    private readonly IFileUploadService _uploadFileService;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public StudentManagementService(Context context,
        IClassManagementService classManagementService,
        IFileUploadService uploadFileService,
        IQrCodeGenerator qrCodeGenerator)
    {
        _context = context;
        _classManagementService = classManagementService;
        _uploadFileService = uploadFileService;
        _qrCodeGenerator = qrCodeGenerator;
    }

    public async Task<IEnumerable<Student>> GetStudents(Guid schoolId)
    {
        return await _context.Students
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsByFilter(Guid schoolId, string? name, Guid? classId)
    {
        var query = await GetStudentsByFilterQueryAble(schoolId, name, classId);

        return await query.ToListAsync();
    }

    public Task<IQueryable<Student>> GetStudentsByFilterQueryAble(Guid schoolId, string? name, Guid? classId)
    {
        var query = _context.Students.AsQueryable()
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(st => EF.Functions.Like(st.FullName, name + "%"));
        if (classId.HasValue)
            query = query.Where(st => st.ClassId == classId);

        return Task.FromResult(query);
    }

    public async Task<Student> GetStudentById(Guid id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null)
            throw new NotFoundException("Không tồn tại học sinh");

        return student;
    }

    public async Task<Student> AddStudent(CreateStudentDto request)
    {
        var trans = await _context.Database.BeginTransactionAsync();

        try
        {
            var cl = await _classManagementService.GetClassById(request.ClassId);
            if (cl.SchoolId != request.SchoolId)
                throw new BadRequestException("Lớp không tồn tại");

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
            
            _context.Students.Add(st);
            _context.Entry(st).State = EntityState.Added;
            _context.Entry(cl).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var hash = HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(st.Id));
            var stream = _qrCodeGenerator.GenerateQrCodeStream(hash);
            var uploadRe = await _uploadFileService.UploadStreamAsync(stream,
                st.Id.ToString() + ".png", "image/png", "student_qr_images");
            st.QrImageKey = uploadRe.Key;

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return st;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<Student> UpdateStudent(UpdateStudentDto request)
    {
        var cl = await _classManagementService.GetClassById(request.ClassId);
        if (cl.SchoolId != request.SchoolId)
            throw new BadRequestException("Lớp không tồn tại");

        var st = await GetStudentById(request.Id);
        var oldClass = await _classManagementService.GetClassById(st.ClassId);
        oldClass.NumberOfStudent -= 1;
        cl.NumberOfStudent += 1;
        
        st.FirstName = request.FirstName;
        st.LastName = request.LastName;
        st.DateOfBirth = request.DateOfBirth;
        st.Gender = request.Gender;
        st.ClassId = request.ClassId;

        _context.Entry(st).State = EntityState.Modified;
        _context.Entry(oldClass).State = EntityState.Modified;
        _context.Entry(cl).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return st;
    }

    public async Task DeleteStudent(Guid id)
    {
        var student = await GetStudentById(id);
        await _context.Entry(student).Reference(x => x.Class).LoadAsync();
        student.Class.NumberOfStudent -= 1;
        _context.Entry(student.Class).State = EntityState.Modified;
        _context.Entry(student).State = EntityState.Deleted;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudent(List<Guid> ids)
    {
        var trans = await _context.Database.BeginTransactionAsync();
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
        if (!(await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.Id == studentId)))
            throw new NotFoundException("Không tồn tại học sinh");
    }

    public async Task IsOwnerOfStudent(Guid schoolId, Guid studentId)
    {
        if (!(await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.Id == studentId)))
            throw new ForbiddenException("Bạn không có quyền truy cập");
    }

    public async Task<string> UploadAvatar(Guid studentId, IFormFile file)
    {
        var student = await GetStudentById(studentId);
        
        if(student.AvatarKey != null)
            await _uploadFileService.DeleteFileAsync(student.AvatarKey.Value);
        var response = await _uploadFileService.UploadFileAsync(file, "avatar/students");

        student.AvatarKey = response.Key;
        _context.Entry(student).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return response.S3Url;
    }
}
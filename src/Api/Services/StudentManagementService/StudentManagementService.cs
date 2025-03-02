using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.StudentManagement;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.Services.UploadFileService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.StudentManagementService;

public class StudentManagementService : IStudentManagementService
{
    private readonly Context _context;
    private readonly IClassManagementService _classManagementService;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _uploadFileService;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public StudentManagementService(Context context,
        IClassManagementService classManagementService,
        IMapper mapper,
        IFileUploadService uploadFileService,
        IQrCodeGenerator qrCodeGenerator)
    {
        _context = context;
        _classManagementService = classManagementService;
        _mapper = mapper;
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

    public async Task<IQueryable<Student>> GetStudentsByFilterQueryAble(Guid schoolId, string? name, Guid? classId)
    {
        var query = _context.Students.AsQueryable()
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(st => EF.Functions.Like(st.FullName, name + "%"));
        if (classId.HasValue)
            query = query.Where(st => st.ClassId == classId);

        return query;
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
            
            _context.Students.Add(st);
            _context.Entry(st).State = EntityState.Added;
            await _context.SaveChangesAsync();
            var hash = HashGenerator.ComputeSHA256("StudentId_" + st.Id.ToString());
            var stream = _qrCodeGenerator.GenerateQrCodeStream(hash);
            var uploadRe = await _uploadFileService.UploadStreamAsync(stream,
                st.Id.ToString() + ".png", "image/png", "student_qr_images");
            st.QrImageKey = uploadRe.Key;

            await _context.SaveChangesAsync();
            await trans.CommitAsync();

            return st;
        }
        catch (Exception e)
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

        st.FirstName = request.FirstName;
        st.LastName = request.LastName;
        st.DateOfBirth = request.DateOfBirth;
        st.Gender = request.Gender;
        st.ClassId = request.ClassId;

        _context.Entry(st).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return st;
    }

    public async Task DeleteStudent(Guid id)
    {
        var student = await GetStudentById(id);
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
        catch (Exception e)
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
}
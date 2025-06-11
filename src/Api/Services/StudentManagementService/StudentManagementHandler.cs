using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ExcelReader;
using Api.DTOs.StudentManagement;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Api.Extensions;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.StudentManagementService;

public class StudentManagementHandler(
    IStudentManagementService studentManagementService,
    IMapper mapper,
    Context context,
    IFileUploadService uploadService,
    IServiceProvider serviceProvider,
    IFileUploadService uploadFileService,
    ILogger<StudentManagementHandler> logger)
    : IStudentManagementHandler
{
    public async Task<Pagination<StudentResponse>> GetStudents(Guid schoolId, GetStudentRequest request)
    {
        var query = await studentManagementService.GetStudentsByFilterQueryAble(schoolId,
            request.StudentId,
            request.Name,
            request.ClassId,
            request.ClassName);
        var total = await query.CountAsync();

        var data = query
            .SortByProperty(request.SortBy, request.Direction)
            .Pagination(request.Page, request.Limit)
            .Select(x => mapper.Map<StudentResponse>(x));

        var response = new Pagination<StudentResponse>(data, request.Limit, request.Page, total);

        return response;
    }

    public async Task<StudentDetailResponse> GetStudentById(Guid schoolId, Guid id)
    {
        await studentManagementService.IsOwnerOfStudent(schoolId, id);
        var student = await studentManagementService.GetStudentById(id);

        return await MapStudent2StudentResponse(student, context, mapper, uploadService);
    }

    public Task<IEnumerable<StudentResponse>> GetMyChildren(Guid parentId)
    {
        throw new NotImplementedException();
    }

    public async Task<StudentDetailResponse> AddStudent(Guid schoolId, CreateStudentRequest request)
    {
        var trans = await context.Database.BeginTransactionAsync();
        await uploadFileService.BeginTransactionAsync();
        try
        {
            var dto = mapper.Map<CreateStudentDto>(request);
            dto.SchoolId = schoolId;
            var student = await studentManagementService.AddStudent(dto);
            await trans.CommitAsync();
            return await MapStudent2StudentResponse(student, context, mapper, uploadService);
        }
        catch (Exception)
        {
            uploadFileService
                .RollBackAsync()
                .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
            throw;
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task<StudentDetailResponse> UpdateStudent(Guid schoolId, UpdateStudentRequest request)
    {
        await studentManagementService.IsOwnerOfStudent(schoolId, request.Id);

        var dto = mapper.Map<UpdateStudentDto>(request);
        dto.SchoolId = schoolId;
        var student = await studentManagementService.UpdateStudent(dto);

        return await MapStudent2StudentResponse(student, context, mapper, uploadService);
    }

    public async Task DeleteStudent(Guid schoolId, Guid id)
    {
        await studentManagementService.IsOwnerOfStudent(schoolId, id);
        await studentManagementService.DeleteStudent(id);
    }

    public async Task DeleteStudent(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids) await studentManagementService.IsOwnerOfStudent(schoolId, id);

        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await studentManagementService.DeleteStudent(ids);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task<string> UploadAvatar(Guid schoolId, Guid studentId, IFormFile file)
    {
        await studentManagementService.IsOwnerOfStudent(schoolId, studentId);
        return await studentManagementService.UploadAvatar(studentId, file);
    }

    public Task<MemoryStream> GetTemplateExcelFile()
    {
        var reader = new ExcelReader<Student>(serviceProvider);
        return Task.FromResult(reader.GetTemplateFile(GetExcelColumnDefinitions()));
    }

    public async Task ImportStudentsFromExcelFile(Guid schoolId, IFormFile file)
    {
        var trans = await context.Database.BeginTransactionAsync();
        await uploadFileService.BeginTransactionAsync();
        try
        {
            var reader = new ExcelReader<Student>(serviceProvider);
            var student = reader.ReadExcel(file.OpenReadStream(), GetExcelColumnDefinitions());
            await studentManagementService.ImportStudentsFromExcel(schoolId, student);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            uploadFileService
                .RollBackAsync()
                .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
            throw;
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    private static async Task<StudentDetailResponse> MapStudent2StudentResponse(Student student, Context context,
        IMapper mapper, IFileUploadService uploadService)
    {
        var entry = context.Entry(student);
        if (!entry.Reference(st => st.School).IsLoaded)
            await context.Entry(student).Reference(s => s.School).LoadAsync();
        if (!entry.Reference(st => st.Class).IsLoaded)
            await context.Entry(student).Reference(s => s.Class).LoadAsync();

        var response = mapper.Map<StudentDetailResponse>(student);

        if (student.QrImageKey != null)
        {
            var key = (await context.FileManagements.FirstOrDefaultAsync(fm => fm.Id == student.QrImageKey))!.S3Key;

            response.QrImageUrl = await uploadService.GeneratePreSignedDownloadUrlAsync(key, 30);
        }

        if (student.AvatarKey != null)
        {
            var key = (await context.FileManagements.FirstOrDefaultAsync(fm => fm.Id == student.AvatarKey))!.S3Key;

            response.AvatarUrl = await uploadService.GeneratePreSignedDownloadUrlAsync(key, 30);
        }

        return response;
    }

    private static List<ExcelColumnDefinition<Student>> GetExcelColumnDefinitions()
    {
        var columns = new List<ExcelColumnDefinition<Student>>
        {
            new(nameof(Student.LastName), "Họ",
                "A", "Phạm"),
            new(nameof(Student.FirstName), "Tên",
                "B", "Văn Tiến Trưởng"),
            new(nameof(Student.DateOfBirth), "Ngày sinh",
                "C", "18/11/2002"),
            new(nameof(Student.ClassId), "Mã lớp",
                "D", "08dd8ac0-4cd0-49be-8aef-1261aeb46acd"),
            new(nameof(Student.Gender), "Giới tính",
                "E", "0"),
            new(nameof(Student.PickUpLocation), "Địa chỉ đưa đón",
                "F", "500 Âu Cơ, Hòa Khánh Bắc, Liên Chiểu, Đà Nẵng")
        };

        return columns;
    }
}
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ExcelReader;
using Api.DTOs.TeacherManagement;
using Api.Extensions;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.TeacherManagementService;

public class TeacherManagementHandler(
    IMapper mapper,
    ITeacherManagementService teacherManagementService,
    IFileUploadService uploadFileService,
    Context context,
    IServiceProvider serviceProvider)
    : ITeacherManagementHandler
{
    public async Task<Pagination<TeacherResponse>> GetTeachers(Guid schoolId, GetTeacherRequest request)
    {
        var query = await teacherManagementService.GetTeachersByFilterQueryAble(schoolId, request.Name, request.Email,
            request.Phone);
        var total = await query.CountAsync();

        var data = query
            .OrderBy(t => t.FullName)
            .Pagination(request.Page, request.Limit)
            .Select(x => mapper.Map<TeacherResponse>(x));

        var response = new Pagination<TeacherResponse>(data, request.Limit, request.Page, total);

        return response;
    }

    public async Task<TeacherDetailResponse> GetTeacherById(Guid schoolId, Guid id)
    {
        await teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        var teacher = await teacherManagementService.GetTeacherById(id);

        return await MapToTeacherResponse(teacher, mapper, uploadFileService);
    }

    public async Task<TeacherDetailResponse> AddTeacher(Guid schoolId, CreateTeacherRequest request)
    {
        var dto = mapper.Map<CreateTeacherDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await teacherManagementService.AddTeacher(dto);

        return await MapToTeacherResponse(teacher, mapper, uploadFileService);
    }

    public async Task<TeacherDetailResponse> UpdateTeacher(Guid schoolId, UpdateTeacherRequest request)
    {
        await teacherManagementService.IsOwnerOfTeacher(schoolId, request.Id);

        var dto = mapper.Map<UpdateTeacherDto>(request);
        var teacher = await teacherManagementService.UpdateTeacher(dto);

        return await MapToTeacherResponse(teacher, mapper, uploadFileService);
    }

    public async Task DeleteTeacher(Guid schoolId, Guid id)
    {
        await teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        await teacherManagementService.DeleteTeacher(id);
    }

    public async Task DeleteTeacher(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids) await teacherManagementService.IsOwnerOfTeacher(schoolId, id);

        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await teacherManagementService.DeleteTeacher(ids);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public async Task<string> UploadAvatar(Guid schoolId, Guid teacherId, IFormFile file)
    {
        await teacherManagementService.IsOwnerOfTeacher(schoolId, teacherId);
        return await teacherManagementService.UploadAvatar(teacherId, file);
    }

    public Task<MemoryStream> GetTemplateExcelFile()
    {
        var reader = new ExcelReader<Teacher>(serviceProvider);
        return Task.FromResult(reader.GetTemplateFile(GetExcelColumnDefinitions()));
    }

    public async Task ImportTeachersFromExcelFile(Guid schoolId, IFormFile file)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            var reader = new ExcelReader<Teacher>(serviceProvider);
            var teachers = reader.ReadExcel(file.OpenReadStream(), GetExcelColumnDefinitions());
            await teacherManagementService.ImportTeachersFromExcel(schoolId, teachers);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    private static async Task<TeacherDetailResponse> MapToTeacherResponse(Teacher teacher,
        IMapper mapper,
        IFileUploadService uploadFileService)
    {
        var response = mapper.Map<TeacherDetailResponse>(teacher);
        if (teacher.AvatarKey != null)
            response.AvatarUrl = await uploadFileService.GeneratePreSignedDownloadUrlAsync(teacher.AvatarKey.Value);
        return response;
    }

    private static List<ExcelColumnDefinition<Teacher>> GetExcelColumnDefinitions()
    {
        var columns = new List<ExcelColumnDefinition<Teacher>>
        {
            new(nameof(Teacher.LastName), "Họ",
                "A", "Nguyễn"),
            new(nameof(Teacher.FirstName), "Tên",
                "B", "Văn A"),
            new(nameof(Teacher.DateOfBirth), "Ngày sinh",
                "C", "01/01/1980"),
            new(nameof(Teacher.Gender), "Giới tính",
                "D", "0"),
            new(nameof(Teacher.PhoneNumber), "Số điện thoại",
                "E", "0123456789"),
            new(nameof(Teacher.Email), "Email",
                "F", "example@example.com")
        };
        return columns;
    }
}
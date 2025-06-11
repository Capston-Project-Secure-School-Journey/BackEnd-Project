using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.DTOs.ExcelReader;
using Api.Extensions;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ClassManagementService;

public class ClassManagementHandler(
    IClassManagementService classManagementService,
    ITeacherManagementService teacherManagementService,
    IMapper mapper,
    IServiceProvider serviceProvider,
    Context context)
    : IClassManagementHandler
{
    public async Task<Pagination<ClassResponse>> GetClasses(Guid schoolId, GetClassesRequest request)
    {
        var query = await classManagementService.GetClassesQueryAbleByFilter(schoolId, request.ClassName,
            request.Grade);
        var total = await query.CountAsync();

        var data = query
            .SortByProperty(request.SortBy, request.Direction)
            .Pagination(request.Page, request.Limit)
            .Select(x => mapper.Map<ClassResponse>(x));

        var response = new Pagination<ClassResponse>(data, request.Limit, request.Page, total);

        return response;
    }

    public async Task<ClassDetailResponse> GetClassById(Guid schoolId, Guid id)
    {
        await classManagementService.IsOwnerOfClass(schoolId, id);
        var cl = await classManagementService.GetClassById(id);
        var response = mapper.Map<ClassDetailResponse>(cl);

        await SetManagedTeachers(response);
        return response;
    }

    public async Task<ClassDetailResponse> AddClass(Guid schoolId, CreateClassRequest request)
    {
        var dto = mapper.Map<CreateClassDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await classManagementService.AddClass(dto);

        var response = mapper.Map<ClassDetailResponse>(teacher);
        await SetManagedTeachers(response);
        return response;
    }

    public async Task<ClassDetailResponse> UpdateClass(Guid schoolId, UpdateClassRequest request)
    {
        await classManagementService.IsOwnerOfClass(schoolId, request.Id);
        var dto = mapper.Map<UpdateClassDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await classManagementService.UpdateClass(dto);

        var response = mapper.Map<ClassDetailResponse>(teacher);
        await SetManagedTeachers(response);

        return response;
    }

    public async Task DeleteClass(Guid schoolId, Guid id)
    {
        await classManagementService.IsOwnerOfClass(schoolId, id);
        await classManagementService.DeleteClass(id);
    }

    public async Task DeleteClass(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids)
            await classManagementService.IsOwnerOfClass(schoolId, id);

        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await classManagementService.DeleteClass(ids);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    public Task<MemoryStream> GetTemplateExcelFile()
    {
        var reader = new ExcelReader<Class>(serviceProvider);
        return Task.FromResult(reader.GetTemplateFile(GetExcelColumnDefinitions()));
    }

    public async Task ImportClassesFromExcelFile(Guid schoolId, IFormFile file)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            var reader = new ExcelReader<Class>(serviceProvider);
            var classes = reader.ReadExcel(file.OpenReadStream(), GetExcelColumnDefinitions());
            await classManagementService.ImportClassesFromExcel(schoolId, classes);
            await trans.CommitAsync();
        }
        finally
        {
            await trans.DisposeAsync();
        }
    }

    private async Task SetManagedTeachers(List<ClassDetailResponse> response)
    {
        var map = new Dictionary<Guid, string>();

        foreach (var managedTeacher in response.SelectMany(cl => cl.ManagedTeachers))
            if (!map.TryGetValue(managedTeacher.Id, out var value))
            {
                var teacher = await teacherManagementService.GetTeacherById(managedTeacher.Id);
                managedTeacher.Name = teacher.FullName;

                map[managedTeacher.Id] = managedTeacher.Name;
            }
            else
            {
                managedTeacher.Name = value;
            }
    }

    private async Task SetManagedTeachers(ClassDetailResponse response)
    {
        var temp = new List<ClassDetailResponse> { response };
        await SetManagedTeachers(temp);
    }

    private static List<ExcelColumnDefinition<Class>> GetExcelColumnDefinitions()
    {
        var columns = new List<ExcelColumnDefinition<Class>>
        {
            new(nameof(Class.ClassName), "Tên lớp",
                "A", "9A"),
            new(nameof(Class.Grade), "Khối",
                "B", "1"),
            new(nameof(Class.ManagedTeachers), "Giáo viên quản lí",
                "C", "08dd8ac1-8fc0-4c50-8f20-27d52b302469,08dd8ac1-c7ac-4958-8493-193af5dbbeeb")
        };

        return columns;
    }
}
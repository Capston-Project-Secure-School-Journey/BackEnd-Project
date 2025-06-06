using Api.DTOs.ClassManagement;
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
    IMapper mapper)
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
        await classManagementService.DeleteClass(ids);
    }

    private async Task SetManagedTeachers(List<ClassDetailResponse> response)
    {
        var map = new Dictionary<Guid, string>();

        foreach (var managedTeacher in response.SelectMany(cl => cl.ManagedTeachers))
            if (!map.TryGetValue(managedTeacher.Id, out var value))
            {
                var teacher = await teacherManagementService.GetTeacherById(managedTeacher.Id);
                managedTeacher.Name = teacher.FirstName + " " + teacher.LastName;

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
}
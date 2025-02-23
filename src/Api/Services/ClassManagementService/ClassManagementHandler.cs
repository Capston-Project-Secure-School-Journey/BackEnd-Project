using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.DTOs.Responses;
using Api.Extensions;
using Api.Services.TeacherManagementService;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ClassManagementService;

public class ClassManagementHandler : IClassManagementHandler
{
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly IClassManagementService _classManagementService;
    private readonly ITeacherManagementService _teacherManagementService;

    public ClassManagementHandler(Context context,
        IClassManagementService classManagementService,
        ITeacherManagementService teacherManagementService,
        IMapper mapper)
    {
        _context = context;
        _classManagementService = classManagementService;
        _teacherManagementService = teacherManagementService;
        _mapper = mapper;
    }

    public async Task<Pagination<ClassResponse>> GetClasses(Guid schoolId, GetClassesRequest request)
    {
        var query = await _classManagementService.GetClassesQueryAbleByFilter(schoolId, request.ClassName, request.Grade);
        var total = await query.CountAsync();

        var data = await query
            .Select(x => _mapper.Map<ClassResponse>(x))
            .Pagination(request.Page, request.Limit)
            .ToListAsync();
        
        await SetManagedTeachers(data);
        var response = new Pagination<ClassResponse>(data, request.Limit, request.Page, total);
        
        return response;
    }

    public async Task<ClassResponse> GetClassById(Guid schoolId, Guid id)
    {
        await _classManagementService.IsOwnerOfClass(schoolId, id);
        var cl = await _classManagementService.GetClassById(id);
        var response = _mapper.Map<ClassResponse>(cl);
        
        await SetManagedTeachers(response);
        return response;
    }

    public async Task<ClassResponse> AddClass(Guid schoolId, CreateClassRequest request)
    {
        var dto = _mapper.Map<CreateClassDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await _classManagementService.AddClass(dto);
        
        var response = _mapper.Map<ClassResponse>(teacher);
        await SetManagedTeachers(response);
        return response;
    }

    public async Task<ClassResponse> UpdateClass(Guid schoolId, UpdateClassRequest request)
    {
        await _classManagementService.IsOwnerOfClass(schoolId, request.Id);
        var dto = _mapper.Map<UpdateClassDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await _classManagementService.UpdateClass(dto);
        
        var response = _mapper.Map<ClassResponse>(teacher);
        await SetManagedTeachers(response);
        
        return response;
    }

    public async Task DeleteClass(Guid schoolId, Guid id)
    {
        await _classManagementService.IsOwnerOfClass(schoolId, id);
        await _classManagementService.DeleteClass(id);
    }

    public async Task DeleteClass(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids)
            await _classManagementService.IsOwnerOfClass(schoolId, id);
        await _classManagementService.DeleteClass(ids);
    }

    private async Task SetManagedTeachers(List<ClassResponse> response)
    {
        var map = new Dictionary<Guid, string>();

        foreach (var managedTeacher in response.SelectMany(cl => cl.ManagedTeachers))
        {
            if (!map.TryGetValue(managedTeacher.Id, out var value))
            {
                var teacher = await _teacherManagementService.GetTeacherById(managedTeacher.Id);
                managedTeacher.Name = teacher.FirstName + " " + teacher.LastName;

                map[managedTeacher.Id] = managedTeacher.Name;
            }
            else
                managedTeacher.Name = value;
        }
    }

    private async Task SetManagedTeachers(ClassResponse response)
    {
        var temp = new List<ClassResponse> { response };
        await SetManagedTeachers(temp);
    }
}
using Api.Common.Enums;
using Api.Domain;
using Api.DTOs.TeacherManagement;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.TeacherManagementService;

public class TeacherManagementHandler: ITeacherManagementHandler
{
    private readonly IMapper _mapper;
    private readonly Context _context;
    private readonly ITeacherManagementService _teacherManagementService;
    
    public TeacherManagementHandler(Context context, 
        IMapper mapper, 
        ITeacherManagementService teacherManagementService)
    {
        _context = context;
        _mapper = mapper;
        _teacherManagementService = teacherManagementService;
    }


    public async Task<Pagination<TeacherResponse>> GetTeachers(Guid schoolId, GetTeacherRequest request)
    {
        var query = await _teacherManagementService.GetTeachersByFilterQueryAble(schoolId, request.Name, request.Email, request.Phone);
        var total = await query.CountAsync();

        var data = await query
            .Select(x => _mapper.Map<TeacherResponse>(x))
            .Pagination(request.Page, request.Limit)
            .ToListAsync();

        var response = new Pagination<TeacherResponse>(data, request.Limit, request.Page, total);
        
        return response;
    }

    public async Task<TeacherResponse> GetTeacherById(Guid schoolId, Guid id)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        var teacher =  await _teacherManagementService.GetTeacherById(id);
        
        return _mapper.Map<TeacherResponse>(teacher);
    }

    public async Task<TeacherResponse> AddTeacher(Guid schoolId, CreateTeacherRequest request)
    {
        var dto = _mapper.Map<CreateTeacherDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await _teacherManagementService.AddTeacher(dto);
        
        var response = _mapper.Map<TeacherResponse>(teacher);
        return response;
    }

    public async Task<TeacherResponse> UpdateTeacher(Guid schoolId, UpdateTeacherRequest request)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, request.Id);
        
        var dto = _mapper.Map<UpdateTeacherDto>(request);
        var teacher = await _teacherManagementService.UpdateTeacher(dto);
        
        var response = _mapper.Map<TeacherResponse>(teacher);
        return response;
    }

    public async Task DeleteTeacher(Guid schoolId, Guid id)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        await _teacherManagementService.DeleteTeacher(id);
    }

    public async Task DeleteTeacher(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await _teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        }
        await _teacherManagementService.DeleteTeacher(ids);
    }

    public async Task<Guid> GetSchoolIdBySchoolAdminId(Guid schoolAdminId)
    {
        var schoolAdmin = await _context.SchoolPersons.FirstOrDefaultAsync(x => x.Id == schoolAdminId && x.UserType == UserType.SchoolAdmin);
        return schoolAdmin!.SchoolId;
    }
}
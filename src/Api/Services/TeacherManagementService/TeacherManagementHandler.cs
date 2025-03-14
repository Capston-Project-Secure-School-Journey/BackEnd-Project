using Api.Common.Enums;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.TeacherManagement;
using Api.Extensions;
using Api.Services.UploadFileService;
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
    private readonly IFileUploadService _uploadFileService;
    public TeacherManagementHandler(Context context, 
        IMapper mapper, 
        ITeacherManagementService teacherManagementService,
        IFileUploadService uploadFileService)
    {
        _context = context;
        _mapper = mapper;
        _teacherManagementService = teacherManagementService;
        _uploadFileService = uploadFileService;
    }


    public async Task<Pagination<TeacherResponse>> GetTeachers(Guid schoolId, GetTeacherRequest request)
    {
        var query = await _teacherManagementService.GetTeachersByFilterQueryAble(schoolId, request.Name, request.Email, request.Phone);
        var total = await query.CountAsync();

        var data = await query
            .Pagination(request.Page, request.Limit)
            .Select(x => _mapper.Map<TeacherResponse>(x))
            .ToListAsync();
        var response = new Pagination<TeacherResponse>(data, request.Limit, request.Page, total);
        
        return response;
    }

    public async Task<TeacherDetailResponse> GetTeacherById(Guid schoolId, Guid id)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, id);
        var teacher =  await _teacherManagementService.GetTeacherById(id);
        
        return await MapToTeacherResponse(teacher, _mapper, _uploadFileService);
    }

    public async Task<TeacherDetailResponse> AddTeacher(Guid schoolId, CreateTeacherRequest request)
    {
        var dto = _mapper.Map<CreateTeacherDto>(request);
        dto.SchoolId = schoolId;
        var teacher = await _teacherManagementService.AddTeacher(dto);
        
        return await MapToTeacherResponse(teacher, _mapper, _uploadFileService);
    }

    public async Task<TeacherDetailResponse> UpdateTeacher(Guid schoolId, UpdateTeacherRequest request)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, request.Id);
        
        var dto = _mapper.Map<UpdateTeacherDto>(request);
        var teacher = await _teacherManagementService.UpdateTeacher(dto);
        
        return await MapToTeacherResponse(teacher, _mapper, _uploadFileService);
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

    public async Task<string> UploadAvatar(Guid schoolId, Guid teacherId, IFormFile file)
    {
        await _teacherManagementService.IsOwnerOfTeacher(schoolId, teacherId);
        return await _teacherManagementService.UploadAvatar(teacherId, file);
    }

    private async Task<TeacherDetailResponse> MapToTeacherResponse(Teacher teacher, 
        IMapper mapper, 
        IFileUploadService uploadFileService)
    {
        var response = mapper.Map<TeacherDetailResponse>(teacher);
        if (teacher.AvatarKey != null)
            response.AvatarUrl = await uploadFileService.GeneratePreSignedDownloadUrlAsync(teacher.AvatarKey.Value);
        return response;
    }
    
}
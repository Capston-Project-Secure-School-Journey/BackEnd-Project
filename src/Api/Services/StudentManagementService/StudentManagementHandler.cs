using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.StudentManagement;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Api.Extensions;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.StudentManagementService;

public class StudentManagementHandler : IStudentManagementHandler
{
    private readonly IStudentManagementService _studentManagementService;
    private readonly IMapper _mapper;
    private readonly Context _context;
    private readonly IFileUploadService _uploadService;

    public StudentManagementHandler(IStudentManagementService studentManagementService,
        IMapper mapper,
        Context context,
        IFileUploadService uploadService)
    {
        _studentManagementService = studentManagementService;
        _mapper = mapper;
        _context = context;
        _uploadService = uploadService;
    }

    public async Task<Pagination<StudentResponse>> GetStudents(Guid schoolId, GetStudentRequest request)
    {
        var query = await _studentManagementService.GetStudentsByFilterQueryAble(schoolId, request.Name,
            request.ClassId);
        var total = await query.CountAsync();

        query = query.Pagination(request.Page, request.Limit);

        var convertTask = query
            .ToList()
            .Select(x => MapStudent2StudentResponse(x, _context, _mapper, _uploadService))
            .ToList();

        var data = await Task.WhenAll(convertTask);

        var response = new Pagination<StudentResponse>(data, request.Limit, request.Page, total);

        return response;
    }

    public async Task<StudentResponse> GetStudentById(Guid schoolId, Guid id)
    {
        await _studentManagementService.IsOwnerOfStudent(schoolId, id);
        var student = await _studentManagementService.GetStudentById(id);

        return await MapStudent2StudentResponse(student, _context, _mapper, _uploadService);
    }

    public Task<IEnumerable<StudentResponse>> GetMyChildren(Guid parentId)
    {
        throw new NotImplementedException();
    }

    public async Task<StudentResponse> AddStudent(Guid schoolId, CreateStudentRequest request)
    {
        var dto = _mapper.Map<CreateStudentDto>(request);
        dto.SchoolId = schoolId;
        var student = await _studentManagementService.AddStudent(dto);

        return await MapStudent2StudentResponse(student, _context, _mapper, _uploadService);
    }

    public async Task<StudentResponse> UpdateStudent(Guid schoolId, UpdateStudentRequest request)
    {
        await _studentManagementService.IsOwnerOfStudent(schoolId, request.Id);

        var dto = _mapper.Map<UpdateStudentDto>(request);
        dto.SchoolId = schoolId;
        var student = await _studentManagementService.UpdateStudent(dto);

        return await MapStudent2StudentResponse(student, _context, _mapper, _uploadService);
    }

    public async Task DeleteStudent(Guid schoolId, Guid id)
    {
        await _studentManagementService.IsOwnerOfStudent(schoolId, id);
        await _studentManagementService.DeleteStudent(id);
    }

    public async Task DeleteStudent(Guid schoolId, List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await _studentManagementService.IsOwnerOfStudent(schoolId, id);
        }

        await _studentManagementService.DeleteStudent(ids);
    }

    private async Task<StudentResponse> MapStudent2StudentResponse(Student student, Context context,
        IMapper mapper, IFileUploadService uploadService)
    {
        var entry = context.Entry(student);
        if (entry.Reference(st => st.School).IsLoaded == false)
            context.Entry(student).Reference(s => s.School).Load();
        if (entry.Reference(st => st.Class).IsLoaded == false)
            context.Entry(student).Reference(s => s.Class).Load();

        var response = mapper.Map<StudentResponse>(student);

        if (student.QrImageKey != null)
        {
            var key = context.FileManagements.FirstOrDefault(fm => fm.Id == student.QrImageKey)!.S3Key;

            response.QrImageUrl = await uploadService.GeneratePreSignedDownloadUrlAsync(key, 30);
        }

        if (student.AvatarKey != null)
        {
            var key = context.FileManagements.FirstOrDefault(fm => fm.Id == student.AvatarKey)!.S3Key;

            response.AvatarUrl = await uploadService.GeneratePreSignedDownloadUrlAsync(key, 30);
        }

        return response;
    }
}
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

public class StudentManagementHandler(
    IStudentManagementService studentManagementService,
    IMapper mapper,
    Context context,
    IFileUploadService uploadService)
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
        var dto = mapper.Map<CreateStudentDto>(request);
        dto.SchoolId = schoolId;
        var student = await studentManagementService.AddStudent(dto);

        return await MapStudent2StudentResponse(student, context, mapper, uploadService);
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

        await studentManagementService.DeleteStudent(ids);
    }

    public async Task<string> UploadAvatar(Guid schoolId, Guid studentId, IFormFile file)
    {
        await studentManagementService.IsOwnerOfStudent(schoolId, studentId);
        return await studentManagementService.UploadAvatar(studentId, file);
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
}
using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.DTOs.SchoolManagement;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using AutoMapper;
using Api.DTOs.Responses;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.IOC.Services.SchoolManagement;

public class SchoolManagementHandler : ISchoolManagementHandler
{
    private readonly ISchoolManagement _schoolManagement;
    private readonly IMapper _mapper;
    private readonly Context _context;
    private readonly IFileUploadService _uploadFileService;

    public SchoolManagementHandler(ISchoolManagement schoolManagement,
        IFileUploadService uploadFileService,
        IMapper mapper,
        Context context)
    {
        _schoolManagement = schoolManagement;
        _mapper = mapper;
        _context = context;
        _uploadFileService = uploadFileService;
    }

    public async Task<SchoolResponse> CreateSchool(CreateSchoolRequest data)
    {
        var school = await _schoolManagement.CreateSchool(_mapper.Map<CreateSchoolDto>(data));
        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task<SchoolResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest data, Guid userRequested,
        UserType userType)
    {
        if (userType == UserType.SchoolAdmin)
        {
            var user = _context.SchoolPersons.FirstOrDefault(sc => sc.Id == userRequested);
            if (user == null || (user != null && user.SchoolId != schoolId))
                throw new ForbiddenException("Access Denied");
        }

        var dto = _mapper.Map<UpdateSchoolDto>(data);
        dto.Id = schoolId;
        var school = await _schoolManagement.UpdateSchool(dto);
        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task DeleteSchool(Guid schoolId)
    {
        await _schoolManagement.DeleteSchool(schoolId);
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        await _schoolManagement.DeleteSchool(schoolIds);
    }

    public async Task<Pagination<SchoolResponse>> GetSchools(GetSchoolRequest request)
    {
        var query = await _schoolManagement.GetSchoolsQueryAble();
        var total = await query.CountAsync();
        
        var data = await query
            .Select(x => _mapper.Map<SchoolResponse>(x))
            .Pagination(request.Page, request.Limit)
            .ToListAsync();
        var tasks = new List<Task>();
        foreach (var school in data)
        {
            school.Images = await GetPreSignedDownload(school.Images);
        }
        
        var response = new Pagination<SchoolResponse>(data, request.Limit, request.Page, total);
        return response;
    }

    public async Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid schoolId)
    {
        var request = new PreSignedUrlRequest()
        {
            ContentType = "image/jpg",
            FileSize = 10 * 1024 * 1024,
            Prefix = "/school/" + schoolId
        };
        var data = await _uploadFileService.GeneratePreSignedUploadUrlAsync(request);
        return data;
    }

    private async Task<List<string>> GetPreSignedDownload(List<string> keys)
    {
        var tasks = keys.
            Select(k => _uploadFileService.GeneratePreSignedDownloadUrlAsync(k));
        
        return (await Task.WhenAll(tasks)).ToList();
    }
}
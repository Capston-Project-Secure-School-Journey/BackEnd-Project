using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.DTOs.SchoolManagement;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using AutoMapper;
using Api.DTOs.Responses;
using Api.DTOs.UploadFileService;
using Api.DTOs.UserManagement;
using Api.Extensions;
using Api.Services.UserManagementService;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.SchoolManagement;

public class SchoolManagementHandler : ISchoolManagementHandler
{
    private readonly ISchoolManagement _schoolManagement;
    private readonly IMapper _mapper;
    private readonly Context _context;
    private readonly IFileUploadService _uploadFileService;
    private readonly IUserManagement _userManagement;

    public SchoolManagementHandler(ISchoolManagement schoolManagement,
        IFileUploadService uploadFileService,
        IUserManagement userManagement,
        IMapper mapper,
        Context context)
    {
        _schoolManagement = schoolManagement;
        _mapper = mapper;
        _context = context;
        _uploadFileService = uploadFileService;
        _userManagement = userManagement;
    }

    public async Task<SchoolDetailResponse> CreateSchool(CreateSchoolRequest data)
    {
        var trans = await _context.Database.BeginTransactionAsync();

        try
        {
            var school = await _schoolManagement.CreateSchool(_mapper.Map<CreateSchoolDto>(data));
            await _userManagement.CreateSchoolAdmin(new CreateSchoolAdminDto()
            {
                UserName = data.SchoolAdminUserName,
                Password = data.SchoolAdminPassword,
                SchoolId = school.Id
            });
            
            var response = _mapper.Map<SchoolDetailResponse>(school);
            response.SchoolAdminUserName = data.SchoolAdminUserName;

            await trans.CommitAsync();
            return response;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<SchoolDetailResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest data, Guid userRequested,
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
        var response = _mapper.Map<SchoolDetailResponse>(school);
        await AttachSchoolAdminInfo(response);
        return response;
    }

    public async Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword)
    {
        await _userManagement.ChangeSchoolAdminPassword(schoolId, newPassword);
    }
    
    public async Task DeleteSchool(Guid schoolId)
    {
        await _schoolManagement.DeleteSchool(schoolId);
        await _userManagement.DeleteSchoolAdmin(schoolId);
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        await _schoolManagement.DeleteSchool(schoolIds);
        await _userManagement.DeleteSchoolAdmin(schoolIds);
    }

    public async Task<Pagination<SchoolResponse>> GetSchools(GetSchoolRequest request)
    {
        var query = await _schoolManagement.GetSchoolsQueryAble();
        var total = await query.CountAsync();

        var data = await query
            .Select(x => _mapper.Map<SchoolResponse>(x))
            .Pagination(request.Page, request.Limit)
            .ToListAsync();

        var response = new Pagination<SchoolResponse>(data, request.Limit, request.Page, total);
        return response;
    }


    public async Task<SchoolDetailResponse> GetSchool(Guid schoolId)
    {
        var school = await _schoolManagement.GetSchool(schoolId);
        var response = _mapper.Map<SchoolDetailResponse>(school);
        response.Images = await GetPreSignedDownload(response.Images);
        await AttachSchoolAdminInfo(response);
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
        var tasks = keys.Select(k => _uploadFileService.GeneratePreSignedDownloadUrlAsync(k));

        return (await Task.WhenAll(tasks)).ToList();
    }

    private async Task AttachSchoolAdminInfo(SchoolDetailResponse school)
    {
        school.SchoolAdminUserName = (await _context.SchoolPersons
                .FirstOrDefaultAsync(x => x.SchoolId == school.Id
                                          && x.UserType == UserType.SchoolAdmin)
            )?.UserName;
    }
}
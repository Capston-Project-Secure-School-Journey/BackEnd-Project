using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.DTOs.SchoolManagement;
using AutoMapper;
using Api.DTOs.UploadFileService;
using Api.DTOs.UserManagement;
using Api.Extensions;
using Api.Services.UserManagementService;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.SchoolManagement;

public class SchoolManagementHandler(
    ISchoolManagement schoolManagement,
    IFileUploadService uploadFileService,
    IUserManagement userManagement,
    IMapper mapper,
    Context context,
    IUserBanService userBanService)
    : ISchoolManagementHandler
{
    public async Task<SchoolDetailResponse> CreateSchool(CreateSchoolRequest request)
    {
        var trans = await context.Database.BeginTransactionAsync();

        try
        {
            var school = await schoolManagement.CreateSchool(mapper.Map<CreateSchoolDto>(request));
            await userManagement.CreateSchoolAdmin(new CreateSchoolAdminDto()
            {
                UserName = request.SchoolAdminUserName,
                Password = request.SchoolAdminPassword,
                SchoolId = school.Id
            });

            var response = mapper.Map<SchoolDetailResponse>(school);
            response.SchoolAdminUserName = request.SchoolAdminUserName;

            await trans.CommitAsync();
            return response;
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<SchoolDetailResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest request, Guid userRequested,
        UserType userType)
    {
        if (userType == UserType.SchoolAdmin)
        {
            var user = await context.SchoolPersons.FirstOrDefaultAsync(sc => sc.Id == userRequested);
            if (user == null || user.SchoolId != schoolId)
                throw new ForbiddenException(ErrorMessages.AccessDenied);
        }

        var dto = mapper.Map<UpdateSchoolDto>(request);
        dto.Id = schoolId;
        var school = await schoolManagement.UpdateSchool(dto);
        var response = mapper.Map<SchoolDetailResponse>(school);
        response.Images = await GetPreSignedDownload(response.Images);
        response.ImageKeys = school.Images.Select(x => x.FileManagementId).ToList();
        await AttachSchoolAdminInfo(response);
        return response;
    }

    public async Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword)
    {
        await userManagement.ChangeSchoolAdminPassword(schoolId, newPassword);
    }

    public async Task IsOwner(Guid schoolId, Guid userId)
    {
        var schoolAdmin = await userManagement.GetSchoolAdmin(schoolId);

        if (schoolAdmin.Id != userId)
            throw new ForbiddenException(ErrorMessages.AccessDenied);
    }

    public async Task DeleteSchool(Guid schoolId)
    {
        await schoolManagement.DeleteSchool(schoolId);
        await userManagement.DeleteSchoolAdmin(schoolId);
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        var trans = await context.Database.BeginTransactionAsync();
        try
        {
            await schoolManagement.DeleteSchool(schoolIds);
            await userManagement.DeleteSchoolAdmin(schoolIds);
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.DisposeAsync();
            throw;
        }
    }

    public async Task<Pagination<SchoolResponse>> GetSchools(GetSchoolRequest request)
    {
        var query = await schoolManagement.GetSchoolsQueryAble();
        var total = await query.CountAsync();

        var data = await query
            .OrderBy(sc => sc.SchoolName)
            .Select(x => mapper.Map<SchoolResponse>(x))
            .Pagination(request.Page, request.Limit)
            .ToListAsync();

        var response = new Pagination<SchoolResponse>(data, request.Limit, request.Page, total);
        return response;
    }


    public async Task<SchoolDetailResponse> GetSchool(Guid schoolId)
    {
        var school = await schoolManagement.GetSchool(schoolId);
        var response = mapper.Map<SchoolDetailResponse>(school);
        response.Images = await GetPreSignedDownload(response.Images);
        response.ImageKeys = school.Images.Select(x => x.FileManagementId).ToList();
        await AttachSchoolAdminInfo(response);
        return response;
    }

    public async Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid userId,
        Guid schoolId,
        string fileName,
        string contentType,
        long fileSize)
    {
        await userBanService.CheckUserBaned(userId, BanType.S3PreSigned, true);

        var request = new PreSignedUrlRequest()
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            Prefix = "school-images/" + schoolId
        };
        var response = await uploadFileService.GeneratePreSignedUploadUrlAsync(request);
        await userBanService.AddErrorRequest(userId, BanType.S3PreSigned);
        return response;
    }

    private async Task<List<string>> GetPreSignedDownload(List<string> keys)
    {
        var tasks = keys.Select(k => uploadFileService.GeneratePreSignedDownloadUrlAsync(k));

        return (await Task.WhenAll(tasks)).ToList();
    }

    private async Task AttachSchoolAdminInfo(SchoolDetailResponse school)
    {
        school.SchoolAdminUserName = (await context.SchoolPersons
                .FirstOrDefaultAsync(x => x.SchoolId == school.Id
                                          && x.UserType == UserType.SchoolAdmin)
            )?.UserName;
    }
}
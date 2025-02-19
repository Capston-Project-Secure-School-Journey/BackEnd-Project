using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.Responses;
using Api.DTOs.UploadFileService;
using Api.Transfers.Requests;
using Api.Transfers.Responses;

namespace Api.IOC.Services.SchoolManagement;

public interface ISchoolManagementHandler
{
    Task<SchoolDetailResponse> CreateSchool(CreateSchoolRequest request);
    Task<SchoolDetailResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest request, Guid userRequested, UserType userType);
    Task DeleteSchool(Guid schoolId);
    Task DeleteSchool(List<Guid> schoolIds);
    Task<Pagination<SchoolResponse>> GetSchools(GetSchoolRequest request);
    Task<SchoolDetailResponse> GetSchool(Guid schoolId);
    Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid schoolId);
    Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword);
}
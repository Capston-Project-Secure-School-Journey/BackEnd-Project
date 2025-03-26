using Api.Common.Enums;
using Api.DTOs.UploadFileService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.SchoolManagement;

public interface ISchoolManagementHandler
{
    Task<SchoolDetailResponse> CreateSchool(CreateSchoolRequest request);

    Task<SchoolDetailResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest request, Guid userRequested,
        UserType userType);

    Task DeleteSchool(Guid schoolId);
    Task DeleteSchool(List<Guid> schoolIds);
    Task<Pagination<SchoolResponse>> GetSchools(GetSchoolRequest request);
    Task<SchoolDetailResponse> GetSchool(Guid schoolId);
    Task<PreSignedUrlResponse> GetPreSignedUploadImage(Guid schoolId);
    Task ChangeSchoolAdminPassword(Guid schoolId, string newPassword);
}
using Api.DTOs.Responses;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;

namespace Api.Services.TeacherManagementService;

public interface ITeacherManagementHandler
{
    Task<Pagination<TeacherResponse>> GetTeachers(Guid schoolId, GetTeacherRequest request);
    Task<TeacherResponse> GetTeacherById(Guid schoolId, Guid id);
    Task<TeacherResponse> AddTeacher(Guid schoolId, CreateTeacherRequest request);
    Task<TeacherResponse> UpdateTeacher(Guid schoolId, UpdateTeacherRequest request);
    Task DeleteTeacher(Guid schoolId, Guid id);
    Task DeleteTeacher(Guid schoolId, List<Guid> ids);
    
    Task<Guid> GetSchoolIdBySchoolAdminId(Guid schoolAdminId);
}
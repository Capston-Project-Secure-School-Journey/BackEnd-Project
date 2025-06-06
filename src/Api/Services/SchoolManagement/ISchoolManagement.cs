using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;

namespace Api.Services.SchoolManagement;

public interface ISchoolManagement
{
    Task<School> CreateSchool(CreateSchoolDto data);
    Task<School> UpdateSchool(UpdateSchoolDto data);
    Task DeleteSchool(Guid schoolId);
    Task DeleteSchool(List<Guid> schoolIds);
    Task<School> GetSchool(Guid schoolId);
    Task<IEnumerable<School>> GetSchoolsByFilter(SchoolType? schoolType = null, string? schoolName = null);
    Task<IQueryable<School>> GetSchoolsQueryAble(SchoolType? schoolType = null, string? schoolName = null);
}
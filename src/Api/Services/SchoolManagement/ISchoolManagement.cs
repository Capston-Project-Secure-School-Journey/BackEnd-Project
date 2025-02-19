using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;

namespace Api.IOC.Services.SchoolManagement;

public interface ISchoolManagement
{
    Task<School> CreateSchool(CreateSchoolDto data);
    Task<School> UpdateSchool(UpdateSchoolDto data);
    Task DeleteSchool(Guid schoolId);
    Task DeleteSchool(List<Guid> schoolIds);
    Task<IEnumerable<School>> GetSchools();
    Task<IEnumerable<School>> GetSchoolsByFilter(SchoolType? schoolType = null, string? schoolName = null);
    Task<IQueryable<School>> GetSchoolsQueryAble(SchoolType? schoolType = null, string? schoolName = null);
}
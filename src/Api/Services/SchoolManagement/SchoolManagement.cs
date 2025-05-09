using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;
using Api.Extensions;
using Api.Services.UploadFileService;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.SchoolManagement;

public class SchoolManagement(
    Context dbContext,
    IFileUploadService fileUploadService)
    : ISchoolManagement
{
    private async Task<School> GetById(Guid id)
    {
        var school = await dbContext.Schools.FirstOrDefaultAsync(s => s.Id == id);

        if (school == null)
            throw new NotFoundException(ErrorMessages.SchoolNotFound);
        return school;
    }

    public async Task<School> CreateSchool(CreateSchoolDto data)
    {
        var school = new School
        {
            SchoolType = data.SchoolType,
            SchoolName = data.SchoolName,
            SchoolDescription = data.SchoolDescription,
            Address = data.Address,
            MorningStartTime = data.MorningStartTime,
            MorningEndTime = data.MorningEndTime,
            AfternoonEndTime = data.AfternoonEndTime,
            AfternoonStartTime = data.AfternoonStartTime,
            Email = data.Email,
            PhoneNumber = data.PhoneNumber
        };

        await dbContext.Schools.AddAsync(school);
        await dbContext.SaveChangesAsync();

        return school;
    }

    public async Task<School> UpdateSchool(UpdateSchoolDto data)
    {
        var school = await GetById(data.Id);

        school.SchoolType = data.SchoolType;
        school.SchoolName = data.SchoolName;
        school.SchoolDescription = data.SchoolDescription;
        school.Address = data.Address;
        school.MorningStartTime = data.MorningStartTime;
        school.MorningEndTime = data.MorningEndTime;
        school.AfternoonEndTime = data.AfternoonEndTime;
        school.AfternoonStartTime = data.AfternoonStartTime;
        school.Email = data.Email;
        school.PhoneNumber = data.PhoneNumber;
        school.Images =
            await RefreshUploadedFileList.RefreshUploadedFiles(school.Images, data.ImageKeys, fileUploadService);
        dbContext.Schools.Update(school);
        await dbContext.SaveChangesAsync();
        return school;
    }

    public async Task DeleteSchool(Guid schoolId)
    {
        var school = await GetById(schoolId);

        dbContext.Schools.Remove(school);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        foreach (var schoolId in schoolIds) await DeleteSchool(schoolId);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<School>> GetSchools()
    {
        return await dbContext.Schools.ToListAsync();
    }

    public async Task<IEnumerable<School>> GetSchoolsByFilter(SchoolType? schoolType = null, string? schoolName = null)
    {
        var query = dbContext.Schools
            .AsQueryable()
            .AsNoTracking();
        if (schoolType != null) query = query.Where(s => s.SchoolType == schoolType);

        if (!string.IsNullOrEmpty(schoolName))
            query = query.Where(s => EF.Functions.Like(s.SchoolName, schoolName + "%"));

        return await query.ToListAsync();
    }

    public Task<IQueryable<School>> GetSchoolsQueryAble(SchoolType? schoolType = null, string? schoolName = null)
    {
        var query = dbContext.Schools
            .AsQueryable()
            .AsNoTracking();

        if (schoolType != null) query = query.Where(s => s.SchoolType == schoolType);

        if (!string.IsNullOrEmpty(schoolName)) query = query.Where(s => s.SchoolName.Contains(schoolName));

        return Task.FromResult(query);
    }

    public async Task<School> GetSchool(Guid schoolId)
    {
        var school = await GetById(schoolId);
        return school;
    }
}
using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.SchoolManagement;

public class SchoolManagement : ISchoolManagement
{
    private readonly Context _context;

    public SchoolManagement(Context dbContext)
    {
        _context = dbContext;
    }

    private async Task<School> GetById(Guid id)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == id);

        if (school == null)
            throw new NotFoundException("School is not found");
        return school;
    }

    public async Task<School> CreateSchool(CreateSchoolDto data)
    {
        var school = new School();
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

        await _context.Schools.AddAsync(school);
        await _context.SaveChangesAsync();

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

        _context.Entry(school).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return school;
    }

    public async Task DeleteSchool(Guid schoolId)
    {
        var school = await GetById(schoolId);

        _context.Entry(school).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        var trans = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var schoolId in schoolIds) await DeleteSchool(schoolId);

            await _context.SaveChangesAsync();
            await trans.CommitAsync();
        }
        catch (Exception)
        {
            await trans.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<School>> GetSchools()
    {
        return await _context.Schools.ToListAsync();
    }

    public async Task<IEnumerable<School>> GetSchoolsByFilter(SchoolType? schoolType = null, string? schoolName = null)
    {
        var query = _context.Schools
            .AsQueryable()
            .AsNoTracking();
        if (schoolType != null) query = query.Where(s => s.SchoolType == schoolType);

        if (!string.IsNullOrEmpty(schoolName))
            query = query.Where(s => EF.Functions.Like(s.SchoolName, schoolName + "%"));

        return await query.ToListAsync();
    }

    public Task<IQueryable<School>> GetSchoolsQueryAble(SchoolType? schoolType = null, string? schoolName = null)
    {
        var query = _context.Schools
            .AsQueryable()
            .AsNoTracking();

        if (schoolType != null) query = query.Where(s => s.SchoolType == schoolType);

        if (!string.IsNullOrEmpty(schoolName)) query = query.Where(s => s.SchoolName.Contains(schoolName));

        return Task.FromResult(query);
    }

    public async Task<School> GetSchool(Guid id)
    {
        var school = await GetById(id);
        return school;
    }
}
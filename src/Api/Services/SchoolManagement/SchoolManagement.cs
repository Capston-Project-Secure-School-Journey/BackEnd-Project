using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;
using Microsoft.EntityFrameworkCore;

namespace Api.IOC.Services.SchoolManagement;

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
        try
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var schoolId in schoolIds)
                {
                    var school = await GetById(schoolId);
                    _context.Entry(school).State = EntityState.Deleted;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async  Task<IEnumerable<School>> GetListOfSchool()
    {
        return await _context.Schools.ToListAsync();
    }
}
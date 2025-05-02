using System.Text;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.TransferDTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ScheduleManagementService;

public class ScheduleManagementService(
    Context context,
    IClassManagementService classManagementService,
    IServiceProvider serviceProvider)
    : IScheduleManagementService
{
    public async Task<IEnumerable<ClassSchedule>> CreateSchedule(Guid schoolId, CreateScheduleDto dto)
    {
        var cts = new CancellationTokenSource();
        var trans = await context.Database.BeginTransactionAsync(cts.Token);
        try
        {
            CheckScheduleDate(dto.Date);
            ValidateCreateScheduleDto(dto);

            var schedules = new List<ClassSchedule>();
            var checkOverlapTasks = new List<Task>();

            switch (dto.ScheduleType)
            {
                case ScheduleType.Grade or ScheduleType.School:
                {
                    var query = context.Classes
                        .Where(c => c.SchoolId == schoolId);

                    if (dto.ScheduleType is ScheduleType.Grade)
                        query = query.Where(c => c.Grade == dto.Grade!.Value);

                    var classes = await query.ToListAsync(cancellationToken: cts.Token);
                    classes = classes.Where(c => !dto.ClassException.Contains(c.Id)).ToList();

                    if (classes.Count > 0)
                    {
                        var scheduleGroup = new ScheduleGroup
                        {
                            SchoolId = schoolId,
                            ScheduleType = dto.ScheduleType,
                            Grade = dto.Grade,
                            SessionType = dto.SessionType,
                            Date = dto.Date,
                            ClassException = dto.ClassException
                        };

                        await context.ScheduleGroups.AddAsync(scheduleGroup, cts.Token);
                        await context.SaveChangesAsync(cts.Token);

                        foreach (var classId in classes.Select(x => x.Id))
                        {
                            checkOverlapTasks.Add(CheckOverlap(schoolId, dto.Date, classId, dto.SessionType,
                                cts.Token));
                            var schedule = new ClassSchedule()
                            {
                                SchoolId = schoolId,
                                ClassId = classId,
                                Date = dto.Date,
                                ScheduleType = dto.ScheduleType,
                                SessionType = dto.SessionType,
                                Grade = dto.ScheduleType == ScheduleType.Grade ? dto.Grade : null,
                                Note = dto.Note,
                                ScheduleGroupId = scheduleGroup.Id
                            };
                            schedules.Add(schedule);
                        }
                    }
                    else
                        throw new BadRequestException(ErrorMessages.NoClassFound);

                    break;
                }
                case ScheduleType.Class:
                {
                    await classManagementService.IsOwnerOfClass(schoolId, dto.ClassId!.Value);
                    var schedule = new ClassSchedule()
                    {
                        SchoolId = schoolId,
                        ClassId = dto.ClassId!.Value,
                        Date = dto.Date,
                        ScheduleType = dto.ScheduleType,
                        SessionType = dto.SessionType,
                        Grade = null,
                        Note = dto.Note,
                        ScheduleGroupId = null
                    };
                    checkOverlapTasks.Add(CheckOverlap(schoolId, dto.Date, dto.ClassId.Value, dto.SessionType,
                        cts.Token));
                    schedules.Add(schedule);
                    break;
                }
            }

            await Task.WhenAll(checkOverlapTasks);

            await context.ClassSchedules.AddRangeAsync(schedules, cts.Token);
            await context.SaveChangesAsync(cts.Token);

            await trans.CommitAsync(cts.Token);
            return schedules;
        }
        catch (Exception)
        {
            await trans.RollbackAsync(cts.Token);
            await cts.CancelAsync();
            cts.Dispose();
            throw;
        }
    }

    public async Task<ClassSchedule> UpdateSchedule(Guid schoolId, UpdateScheduleDto dto)
    {
        CheckScheduleDate(dto.Date);
        var schedule = await context.ClassSchedules
            .Include(c => c.Class)
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.SchoolId == schoolId);

        if (schedule == null)
            throw new BadRequestException(ErrorMessages.ScheduleNotFound);

        await CheckOverlap(schoolId, dto.Date, dto.ClassId, dto.SessionType, CancellationToken.None);
        if (dto.ClassId != schedule.ClassId ||
            dto.Date != schedule.Date ||
            dto.SessionType != schedule.SessionType)
        {
            var group = FindScheduleGroupMatchWithSchedule(
                schedule.SchoolId,
                dto.Date,
                dto.SessionType,
                schedule.Class.Grade);

            if (group != null)
            {
                schedule.ScheduleGroupId = group.Id;
                schedule.ScheduleType = group.ScheduleType;

                if (group.ScheduleType is ScheduleType.Grade)
                    schedule.Grade = group.Grade;
            }
            else
            {
                schedule.ScheduleType = ScheduleType.Class;
                schedule.ScheduleGroupId = null;
                schedule.Grade = null;
            }
        }

        schedule.SessionType = dto.SessionType;
        schedule.ClassId = dto.ClassId;
        schedule.Note = dto.Note;
        schedule.Date = dto.Date;
        context.Entry(schedule).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return schedule;
    }

    public async Task DeleteSchedule(Guid schoolId, Guid id)
    {
        var schedule = await context.ClassSchedules
            .Include(x => x.ScheduleGroup)
            .FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == schoolId);

        if (schedule == null)
            throw new BadRequestException(ErrorMessages.ScheduleNotFound);

        if (schedule.ScheduleGroupId != null)
        {
            schedule.ScheduleGroup!.ClassException.Add(schedule.ClassId);

            int classCountByGrade;
            if (schedule.ScheduleType is ScheduleType.Grade)
                classCountByGrade = await context.ClassSchedules
                    .CountAsync(x => x.SchoolId == schoolId && x.Grade == schedule.Grade);
            else
                classCountByGrade = await context.ClassSchedules
                    .CountAsync(x => x.SchoolId == schoolId);

            if (schedule.ScheduleGroup!.ClassException.Count == classCountByGrade)
            {
                context.ScheduleGroups.Remove(schedule.ScheduleGroup!);
                context.Entry(schedule.ScheduleGroup!).State = EntityState.Deleted;
            }
        }

        context.ClassSchedules.Remove(schedule);
        context.Entry(schedule).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }

    public async Task DeleteSchedule(Guid schoolId, List<Guid> ids)
    {
        for (int i = 0; i < ids.Count - 1; i++)
        {
            await DeleteSchedule(schoolId, ids[i]);
        }
    }

    public Task<IEnumerable<ClassSchedule>> GetScheduleByWeek(Guid schoolId, DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ClassSchedule>> GetScheduleByDate(Guid schoolId, DateOnly date)
    {
        var schedules = await context.ClassSchedules
            .Include(c => c.Class)
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId &&
                        c.Date == date)
            .OrderBy(c => c.Class.Grade)
            .ThenByDescending(c => c.Class.ClassName)
            .ToListAsync();

        return schedules;
    }

    public async Task<ClassSchedulePaginationResponse> GetScheduleView(Guid schoolId, DateOnly date)
    {
        var monthRange = DateTimeHelper.GetMonthRange(date);

        var scheduleGroups = await context.ScheduleGroups
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId &&
                        c.Date >= monthRange.StartOfMonth &&
                        c.Date <= monthRange.EndOfMonth)
            .OrderBy(c => c.Date)
            .ThenByDescending(c => c.ScheduleType)
            .ThenBy(c => c.SessionType)
            .ToListAsync();

        var classNames = await context.Classes
            .AsNoTracking()
            .Where(x => x.SchoolId == schoolId)
            .Select(x => new { x.Id, x.ClassName })
            .ToListAsync();

        var response = new ClassSchedulePaginationResponse();

        foreach (var group in scheduleGroups)
        {
            var classResponse = new ClassScheduleResponseView();
            if (group.ScheduleType == ScheduleType.Grade)
            {
                classResponse.ClassId = null;
                classResponse.ClassName = string.Empty;
                classResponse.Date = group.Date;
                classResponse.SessionType = group.SessionType;
                classResponse.ScheduleType = group.ScheduleType;
                classResponse.Grade = group.Grade;
                classResponse.ClassException = group.ClassException;
                classResponse.ClassNameException = group.ClassException
                    .Select(x => classNames.First(k => k.Id == x).ClassName)
                    .ToList();
            }
            else if (group.ScheduleType == ScheduleType.School)
            {
                classResponse.ClassId = null;
                classResponse.ClassName = string.Empty;
                classResponse.Date = group.Date;
                classResponse.SessionType = group.SessionType;
                classResponse.ScheduleType = group.ScheduleType;
                classResponse.Grade = null;
                classResponse.ClassException = group.ClassException;
                classResponse.ClassException = group.ClassException;
                classResponse.ClassNameException = group.ClassException
                    .Select(x => classNames.First(k => k.Id == x).ClassName)
                    .ToList();
            }

            if (response.ClassSchedules.TryGetValue(group.Date, out var value))
            {
                value.Add(classResponse);
            }
            else
                response.ClassSchedules.Add(group.Date, new List<ClassScheduleResponseView>() { classResponse });
        }

        var schedules = await context.ClassSchedules
            .Include(c => c.Class)
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId &&
                        c.Date >= monthRange.StartOfMonth &&
                        c.Date <= monthRange.EndOfMonth &&
                        c.ScheduleType == ScheduleType.Class)
            .OrderBy(c => c.Date)
            .ThenByDescending(c => c.ScheduleType)
            .ThenBy(c => c.SessionType)
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            var classResponse = new ClassScheduleResponseView
            {
                ClassId = schedule.ClassId,
                ClassName = schedule.Class.ClassName,
                Date = schedule.Date,
                SessionType = schedule.SessionType,
                ScheduleType = schedule.ScheduleType,
                Note = schedule.Note,
                Grade = null
            };

            if (response.ClassSchedules.TryGetValue(schedule.Date, out var value))
            {
                value.Add(classResponse);
            }
            else
                response.ClassSchedules.Add(schedule.Date, new List<ClassScheduleResponseView>() { classResponse });
        }

        foreach (var k in response.ClassSchedules.Keys)
        {
            response.ClassSchedules[k] = response.ClassSchedules[k]
                .OrderByDescending(c => c.ScheduleType)
                .ThenByDescending(c => c.SessionType)
                .ToList();
        }

        return response;
    }

    private ScheduleGroup? FindScheduleGroupMatchWithSchedule(Guid schoolId,
        DateOnly date,
        SessionType sessionType,
        Grade grade)
    {
        var schoolGroup = context.ScheduleGroups
            .FirstOrDefault(g => g.SchoolId == schoolId
                                 && g.Date == date
                                 && g.SessionType == sessionType);

        if (schoolGroup != null) return schoolGroup;

        var gradeGroup = context.ScheduleGroups
            .FirstOrDefault(g => g.SchoolId == schoolId
                                 && g.Date == date
                                 && g.SessionType == sessionType
                                 && g.Grade == grade);

        return gradeGroup;
    }

    public Task<IEnumerable<ClassSchedule>> CloneMonthSchedule(Guid schoolId, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ClassSchedule>> CloneWeekSchedule(Guid schoolId, DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ClassSchedule>> CloneDaySchedule(Guid schoolId, DateOnly date)
    {
        throw new NotImplementedException();
    }

    private async Task CheckOverlap(Guid schoolId,
        DateOnly date,
        Guid classId,
        SessionType sessionType,
        CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        var builder = new StringBuilder();
        using var scope = serviceProvider.CreateScope();
        var ct = scope.ServiceProvider.GetRequiredService<Context>();

        var query = ct.ClassSchedules
            .Where(c => c.SchoolId == schoolId
                        && c.ClassId == classId
                        && c.Date == date
            );
        if (sessionType != SessionType.FullDay)
            query = query.Where(c =>
                (c.SessionType == sessionType ||
                 c.SessionType == SessionType.FullDay));
        if (await query.AnyAsync(cancellationToken: token))
        {
            if (token.IsCancellationRequested)
                return;

            var className = await ct.Classes
                .Where(cl => cl.SchoolId == schoolId && cl.Id == classId)
                .Select(c => c.ClassName)
                .FirstOrDefaultAsync(cancellationToken: token);
            var schedule = await query.FirstOrDefaultAsync(cancellationToken: token);

            builder.Append("Lịch của bạn bị trùng.\n");
            builder.Append($"Lớp: {className}\n");
            builder.Append($"Ngày học bị trùng: {date}\n");
            builder.Append($"Đã có lịch học {schedule?.SessionType.GetEnumDisplayName()}\n");
            builder.Append($"Vui lòng kiểm tra lại!");

            if (token.IsCancellationRequested)
                return;
            throw new BadRequestException(builder.ToString());
        }
    }

    private static void CheckScheduleDate(DateOnly date)
    {
        var currentDate = DateOnly.FromDateTime(DateTimeHelper.GetDateTimeUtc7());
        var weekRange = DateTimeHelper.GetWeekRange(date);

        if (currentDate > date)
            throw new BadRequestException(ErrorMessages.CannotAddPastSchedule);
        if (weekRange.StartOfWeek.AddDays(-1) <= currentDate)
            throw new BadRequestException(ErrorMessages.InvalidScheduleAddTime);
    }

    private static void ValidateCreateScheduleDto(CreateScheduleDto dto)
    {
        if (dto is { ScheduleType: ScheduleType.Class, ClassId: null })
            throw new BadRequestException(ErrorMessages.ClassIsEmpty);
        if (dto is { ScheduleType: ScheduleType.Grade, Grade: null })
            throw new BadRequestException(ErrorMessages.GradeIsEmpty);
    }
}
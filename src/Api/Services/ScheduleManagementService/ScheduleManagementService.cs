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
    IClassManagementService classManagementService)
    : IScheduleManagementService
{
    public async Task<IEnumerable<ClassSchedule>> CreateSchedule(Guid schoolId, CreateScheduleDto dto)
    {
        CheckScheduleDate(dto.Date);
        ValidateCreateScheduleDto(dto);

        var schedules = new List<ClassSchedule>();

        switch (dto.ScheduleType)
        {
            case ScheduleType.Grade or ScheduleType.School:
            {
                var classes = await GetTargetClasses(schoolId, dto.ScheduleType, dto.Grade, dto.ClassException);
                await CheckOverlap(schoolId,
                    dto.Date,
                    classes,
                    dto.SessionType);

                var searchResult = await FindTargetGroupToMergeInto(schoolId, dto.Date, dto.SessionType, classes);
                var destinationGroup = searchResult.Item1;
                if (destinationGroup == null)
                {
                    destinationGroup = new ScheduleGroup
                    {
                        SchoolId = schoolId,
                        ScheduleType = dto.ScheduleType,
                        Grade = dto.Grade,
                        SessionType = dto.SessionType,
                        Date = dto.Date,
                        ClassException = dto.ClassException
                    };

                    await context.ScheduleGroups.AddAsync(destinationGroup);
                }
                else
                {
                    if (searchResult.isFullDay)
                    {
                        destinationGroup.SessionType = SessionType.FullDay;
                        context.ClassSchedules.RemoveRange(destinationGroup.ClassSchedules);
                    }
                    else
                        destinationGroup.ClassException = destinationGroup.ClassException.Except(classes).ToList();

                    context.ScheduleGroups.Update(destinationGroup);
                }

                await context.SaveChangesAsync();

                foreach (var classId in classes)
                {
                    var schedule = new ClassSchedule()
                    {
                        SchoolId = schoolId,
                        ClassId = classId,
                        Date = dto.Date,
                        ScheduleType = destinationGroup.ScheduleType,
                        SessionType = destinationGroup.SessionType,
                        Grade = destinationGroup.Grade,
                        Note = dto.Note,
                        ScheduleGroupId = destinationGroup.Id
                    };
                    schedules.Add(schedule);
                }

                break;
            }
            case ScheduleType.Class:
            {
                await classManagementService.IsOwnerOfClass(schoolId, dto.ClassId!.Value);
                await CheckOverlap(schoolId, dto.Date, dto.ClassId.Value, dto.SessionType);
                var grade = await context.Classes
                    .Where(x => x.Id == dto.ClassId.Value).Select(x => x.Grade).FirstAsync();

                var matchGroup = FindScheduleGroupMatchWithSchedule(
                    schoolId,
                    dto.Date,
                    dto.SessionType,
                    grade
                );

                if (matchGroup != null)
                {
                    matchGroup.ClassException.Remove(dto.ClassId.Value);
                    context.ScheduleGroups.Update(matchGroup);
                }

                var schedule = new ClassSchedule()
                {
                    SchoolId = schoolId,
                    ClassId = dto.ClassId!.Value,
                    Date = dto.Date,
                    ScheduleType = matchGroup?.ScheduleType ?? dto.ScheduleType,
                    SessionType = dto.SessionType,
                    Grade = matchGroup?.Grade,
                    Note = dto.Note,
                    ScheduleGroupId = matchGroup?.Id
                };
                schedules.Add(schedule);
                break;
            }
        }

        await context.ClassSchedules.AddRangeAsync(schedules);
        await context.SaveChangesAsync();

        return schedules;
    }

    public async Task<ClassSchedule> UpdateSchedule(Guid schoolId, UpdateScheduleDto dto)
    {
        CheckScheduleDate(dto.Date);
        var schedule = await context.ClassSchedules
            .Include(c => c.Class)
            .Include(c => c.ScheduleGroup)
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.SchoolId == schoolId);

        if (schedule == null)
            throw new BadRequestException(ErrorMessages.ScheduleNotFound);

        var isUnchanged =
            (dto.Date, dto.ClassId, dto.SessionType) ==
            (schedule.Date, schedule.ClassId, schedule.SessionType);

        if (!isUnchanged)
        {
            await CheckOverlap(schoolId, dto.Date, dto.ClassId, dto.SessionType);
            await DetachScheduleFromGroup(schedule);

            var matchGroup = FindScheduleGroupMatchWithSchedule(
                schedule.SchoolId,
                dto.Date,
                dto.SessionType,
                schedule.Class.Grade);

            if (matchGroup != null)
            {
                schedule.ScheduleGroupId = matchGroup.Id;
                schedule.ScheduleType = matchGroup.ScheduleType;

                if (matchGroup.ScheduleType is ScheduleType.Grade)
                    schedule.Grade = matchGroup.Grade;

                // remove schedule from match group
                matchGroup.ClassException.Remove(schedule.ClassId);
                context.ScheduleGroups.Update(matchGroup);
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
        context.ClassSchedules.Update(schedule);
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
        await DetachScheduleFromGroup(schedule);

        context.ClassSchedules.Remove(schedule);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSchedule(Guid schoolId, List<Guid> ids)
    {
        for (var i = 0; i < ids.Count - 1; i++) await DeleteSchedule(schoolId, ids[i]);
    }

    public Task<IEnumerable<ClassSchedule>> GetScheduleByWeek(Guid schoolId, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<IQueryable<ClassSchedule>> GetScheduleByDateQueryable(Guid schoolId,
        DateOnly date,
        SessionType? sessionType,
        Guid? classId,
        string? className,
        Grade? grade)
    {
        var query = context
            .ClassSchedules
            .Include(c => c.Class)
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.Date == date);

        if (classId.HasValue)
            query = query.Where(c => c.ClassId == classId.Value);

        if (!string.IsNullOrWhiteSpace(className))
            query = query.Where(c => EF.Functions.Like(c.Class.ClassName, className + "%"));

        if (sessionType.HasValue)
            query = query.Where(c => c.SessionType == sessionType.Value);

        if (grade.HasValue)
            query = query.Where(c => c.Class.Grade == grade.Value);

        return Task.FromResult(query);
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
        var grades = await GetGrades(schoolId);

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
                var classException = group.ClassException;
                classResponse.GradeException = GetGradeException(ref classException, grades);
                classResponse.ClassException = classException;
                classResponse.ClassNameException = classException
                    .Select(x => classNames.First(k => k.Id == x).ClassName)
                    .ToList();
            }

            if (response.ClassSchedules.TryGetValue(group.Date, out var value))
                value.Add(classResponse);
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
                value.Add(classResponse);
            else
                response.ClassSchedules.Add(schedule.Date, new List<ClassScheduleResponseView>() { classResponse });
        }

        foreach (var k in response.ClassSchedules.Keys)
            response.ClassSchedules[k] = response.ClassSchedules[k]
                .OrderByDescending(c => c.ScheduleType)
                .ThenByDescending(c => c.SessionType)
                .ToList();

        return response;
    }

    private async Task DetachScheduleFromGroup(ClassSchedule schedule)
    {
        var entity = context.Entry(schedule);
        if (!entity.Reference(x => x.ScheduleGroup).IsLoaded)
            await entity.Reference(x => x.ScheduleGroup).LoadAsync();

        if (schedule.ScheduleGroup != null)
        {
            schedule.ScheduleGroup.ClassException.Add(schedule.ClassId);

            int classCountByGrade;
            if (schedule.ScheduleType is ScheduleType.Grade)
                classCountByGrade = await context.ClassSchedules
                    .CountAsync(x => x.SchoolId == schedule.SchoolId && x.Grade == schedule.Grade);
            else
                classCountByGrade = await context.ClassSchedules
                    .CountAsync(x => x.SchoolId == schedule.SchoolId);

            context.Entry(schedule.ScheduleGroup).State =
                schedule.ScheduleGroup.ClassException.Count == classCountByGrade
                    ? EntityState.Deleted
                    : EntityState.Modified;
        }
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

    private async Task<List<Guid>> GetTargetClasses(Guid schoolId, ScheduleType scheduleType, Grade? grade,
        List<Guid> classException)
    {
        var query = context.Classes
            .Where(c => c.SchoolId == schoolId);

        if (scheduleType is ScheduleType.Grade)
            query = query.Where(c => c.Grade == grade!.Value);

        var classes = await query
            .Select(x => x.Id)
            .ToListAsync();

        classes = classes.Where(id => !classException.Contains(id)).ToList();

        if (classes.Count == 0)
            throw new BadRequestException(ErrorMessages.NoClassFound);

        return classes;
    }

    private Task<IQueryable<ClassSchedule>> GetOverlapQueryable(Guid schoolId,
        DateOnly date,
        Guid classId,
        SessionType sessionType
    )
    {
        var query = context.ClassSchedules
            .AsNoTracking()
            .AsQueryable()
            .Where(c => c.SchoolId == schoolId
                        && c.ClassId == classId
                        && c.Date == date
            );

        if (sessionType != SessionType.FullDay)
            query = query.Where(c =>
                c.SessionType == sessionType ||
                c.SessionType == SessionType.FullDay);

        return Task.FromResult(query);
    }

    private async Task CheckOverlap(Guid schoolId,
        DateOnly date,
        Guid classId,
        SessionType sessionType
    )
    {
        await CheckOverlap(schoolId, date, [classId], sessionType);
    }

    private async Task CheckOverlap(Guid schoolId,
        DateOnly date,
        List<Guid> classIds,
        SessionType sessionType
    )
    {
        if (classIds.Count == 0)
            return;

        IQueryable<ClassSchedule> query = null!;
        foreach (var classId in classIds)
        {
            if (query == null)
                query = await GetOverlapQueryable(schoolId, date, classId, sessionType);
            else
                query = query.Union(await GetOverlapQueryable(schoolId, date, classId, sessionType));
        }

        if (await query.AnyAsync())
        {
            var overlapSchedule = await query
                .Include(x => x.Class)
                .FirstOrDefaultAsync();

            throw new BadRequestException(BuildOverlapErrorMessage(overlapSchedule!));
        }
    }

    private async Task<Dictionary<Grade, List<Guid>>> GetGrades(Guid schoolId)
    {
        var grades = new Dictionary<Grade, List<Guid>>();

        var classes = await context.Classes
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .Select(c => new { c.Id, c.Grade })
            .ToListAsync();

        foreach (var cl in classes)
        {
            if (grades.TryGetValue(cl.Grade, out var value))
                value.Add(cl.Id);
            else
                grades.Add(cl.Grade, [cl.Id]);
        }

        return grades;
    }

    private static List<Grade> GetGradeException(ref List<Guid> classException, Dictionary<Grade, List<Guid>> grades)
    {
        var gradeException = new List<Grade>();
        foreach (var grade in grades)
        {
            var flag = true;
            foreach (var classId in grade.Value)
            {
                if (!classException.Contains(classId))
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                classException = classException.Except(grade.Value).ToList();
                gradeException.Add(grade.Key);
            }
        }

        return gradeException;
    }

    private static string BuildOverlapErrorMessage(ClassSchedule overlapSchedule)
    {
        var builder = new StringBuilder();

        builder.Append("Lịch của bạn bị trùng.\n");
        builder.Append($"Lớp: {overlapSchedule.Class.ClassName}\n");
        builder.Append($"Ngày học bị trùng: {overlapSchedule.Date}\n");
        builder.Append($"Đã có lịch học {overlapSchedule.SessionType.GetEnumDisplayName()}\n");
        builder.Append($"Vui lòng kiểm tra lại!");

        return builder.ToString();
    }

    private Task<(ScheduleGroup?, bool isFullDay)> FindTargetGroupToMergeInto(Guid schoolId,
        DateOnly date,
        SessionType sessionType,
        List<Guid> classes
    )
    {
        var isFullDay = false;
        var group = context.ScheduleGroups
            .Include(g => g.ClassSchedules)
            .Where(sc => sc.SchoolId == schoolId
                         && sc.Date == date
                         && sc.SessionType == sessionType)
            .OrderByDescending(sc => sc.ScheduleType)
            .AsEnumerable()
            .FirstOrDefault(g => classes.All(x => g.ClassException.Contains(x)));

        if (group == null && sessionType != SessionType.FullDay)
        {
            group = context.ScheduleGroups
                .Include(g => g.ClassSchedules)
                .Where(sc => sc.SchoolId == schoolId
                             && sc.Date == date
                             && sc.SessionType != SessionType.FullDay
                             && sc.SessionType != sessionType)
                .AsEnumerable()
                .FirstOrDefault(x => x
                    .ClassSchedules
                    .Select(cs => cs.ClassId)
                    .Order()
                    .SequenceEqual(classes.Order())
                );

            if (group != null)
                isFullDay = true;
        }

        return Task.FromResult((group, isFullDay));
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
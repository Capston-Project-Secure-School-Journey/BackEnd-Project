using System.Text;
using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.Extensions;
using Api.Services.ClassManagementService;
using Api.TransferDTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ScheduleManagementService;

public class ScheduleManagementService : IScheduleManagementService
{
    private readonly Context _context;
    private readonly IClassManagementService _classManagementService;
    private readonly IServiceProvider _serviceProvider;

    public ScheduleManagementService(Context context,
        IClassManagementService classManagementService,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _classManagementService = classManagementService;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<ClassSchedule>> CreateSchedule(Guid schoolId, CreateScheduleDto dto)
    {
        CheckScheduleDate(dto.Date);

        if (dto is { ScheduleType: ScheduleType.Class, ClassId: null })
            throw new BadRequestException("Lớp học bị trống.");
        if (dto is { ScheduleType: ScheduleType.Grade, Grade: null })
            throw new BadRequestException("Khối bị trống.");

        var schedules = new List<ClassSchedule>();
        var checkOverlapTasks = new List<Task>();
        var cts = new CancellationTokenSource();

        if (dto.ScheduleType is ScheduleType.Grade or ScheduleType.School)
        {
            var query = _context.Classes
                .Where(c => c.SchoolId == schoolId);

            if (dto.ScheduleType is ScheduleType.Grade)
                query = query.Where(c => c.Grade == dto.Grade!.Value);

            // ReSharper disable once MethodSupportsCancellation
            var classes = await query.ToListAsync();
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

                // ReSharper disable once MethodSupportsCancellation
                await _context.ScheduleGroups.AddAsync(scheduleGroup);
                _context.Entry(scheduleGroup).State = EntityState.Added;
                // ReSharper disable once MethodSupportsCancellation
                await _context.SaveChangesAsync();

                foreach (var cl in classes)
                {
                    checkOverlapTasks.Add(CheckOverlap(schoolId, dto.Date, cl.Id, dto.SessionType, cts.Token));
                    var schedule = new ClassSchedule()
                    {
                        SchoolId = schoolId,
                        ClassId = cl.Id,
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
            {
                if (dto.ScheduleType is ScheduleType.School)
                    throw new BadRequestException(
                        $"Không có lớp nào được tìm thấy trong trường");
                else
                    throw new BadRequestException(
                        $"Không có lớp nào được tìm thấy trong khối {dto.ScheduleType.GetDescription()}");
            }
        }
        else if (dto.ScheduleType == ScheduleType.Class)
        {
            await _classManagementService.IsOwnerOfClass(schoolId, dto.ClassId!.Value);
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
            checkOverlapTasks.Add(CheckOverlap(schoolId, dto.Date, dto.ClassId.Value, dto.SessionType, cts.Token));
            schedules.Add(schedule);
        }

        try
        {
            await Task.WhenAll(checkOverlapTasks);
        }
        catch (Exception)
        {
            await cts.CancelAsync();
            throw;
        }

        // ReSharper disable once MethodSupportsCancellation
        await _context.ClassSchedules.AddRangeAsync(schedules);
        foreach (var schedule in schedules)
            _context.Entry(schedule).State = EntityState.Added;
        // ReSharper disable once MethodSupportsCancellation
        await _context.SaveChangesAsync();

        return schedules;
    }

    public async Task<ClassSchedule> UpdateSchedule(Guid schoolId, UpdateScheduleDto dto)
    {
        CheckScheduleDate(dto.Date);
        var schedule = await _context.ClassSchedules
            .Include(c => c.Class)
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.SchoolId == schoolId);

        if (schedule == null)
            throw new BadRequestException("Không tìm thấy lịch học");

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
        _context.Entry(schedule).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task DeleteSchedule(Guid schoolId, Guid id)
    {
        var schedule = await _context.ClassSchedules
            .Include(x => x.ScheduleGroup)
            .FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == schoolId);

        if (schedule == null)
            throw new BadRequestException("Không tìm thấy lịch học");

        if (schedule.ScheduleGroupId != null)
        {
            schedule.ScheduleGroup!.ClassException.Add(schedule.ClassId);

            int classCountByGrade;
            if (schedule.ScheduleType is ScheduleType.Grade)
                classCountByGrade = _context.ClassSchedules
                    .Count(x => x.SchoolId == schoolId && x.Grade == schedule.Grade);
            else
                classCountByGrade = _context.ClassSchedules
                    .Count(x => x.SchoolId == schoolId);

            if (schedule.ScheduleGroup!.ClassException.Count == classCountByGrade)
            {
                _context.ScheduleGroups.Remove(schedule.ScheduleGroup!);
                _context.Entry(schedule.ScheduleGroup!).State = EntityState.Deleted;
            }
        }

        _context.ClassSchedules.Remove(schedule);
        _context.Entry(schedule).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
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
        var schedules = await _context.ClassSchedules
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
        var monthRange = GetMonthRange(date);

        var scheduleGroups = await _context.ScheduleGroups
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId &&
                        c.Date >= monthRange.StartOfMonth &&
                        c.Date <= monthRange.EndOfMonth)
            .OrderBy(c => c.Date)
            .ThenByDescending(c => c.ScheduleType)
            .ThenBy(c => c.SessionType)
            .ToListAsync();

        var classNames = await _context.Classes
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

            // ReSharper disable once UnusedVariable
            if (response.ClassSchedules.TryGetValue(group.Date, out var a))
            {
                response.ClassSchedules[group.Date] = response.ClassSchedules[group.Date].Append(classResponse);
            }
            else
                response.ClassSchedules.Add(group.Date, new List<ClassScheduleResponseView>() { classResponse });
        }

        var schedules = await _context.ClassSchedules
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

            // ReSharper disable once UnusedVariable
            if (response.ClassSchedules.TryGetValue(schedule.Date, out var a))
            {
                response.ClassSchedules[schedule.Date] = response.ClassSchedules[schedule.Date].Append(classResponse);
            }
            else
                response.ClassSchedules.Add(schedule.Date, new List<ClassScheduleResponseView>() { classResponse });
        }

        foreach (var k in response.ClassSchedules.Keys)
        {
            response.ClassSchedules[k] = response.ClassSchedules[k]
                .OrderByDescending(c => c.ScheduleType)
                .ThenByDescending(c => c.SessionType);
        }

        return response;
    }

    private ScheduleGroup? FindScheduleGroupMatchWithSchedule(Guid schoolId,
        DateOnly date,
        SessionType sessionType,
        Grade grade)
    {
        var schoolGroup = _context.ScheduleGroups
            .FirstOrDefault(g => g.SchoolId == schoolId
                                 && g.Date == date
                                 && g.SessionType == sessionType);

        if (schoolGroup != null) return schoolGroup;

        var gradeGroup = _context.ScheduleGroups
            .FirstOrDefault(g => g.SchoolId == schoolId
                                 && g.Date == date
                                 && g.SessionType == sessionType
                                 && g.Grade == grade);

        if (gradeGroup != null) return gradeGroup;

        return null;
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

        StringBuilder builder = new StringBuilder();
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();

        var query = context.ClassSchedules
            .Where(c => c.SchoolId == schoolId
                        && c.ClassId == classId
                        && c.Date == date
            );
        if (sessionType != SessionType.FullDay)
            query = query.Where(c =>
                (c.SessionType == sessionType ||
                 c.SessionType == SessionType.FullDay));
        if (query.Any())
        {
            if (token.IsCancellationRequested)
                return;

            var className = await context.Classes
                .Where(cl => cl.SchoolId == schoolId && cl.Id == classId)
                .Select(c => c.ClassName)
                // ReSharper disable once MethodSupportsCancellation
                .FirstOrDefaultAsync();
            // ReSharper disable once MethodSupportsCancellation
            var schedule = await query.FirstOrDefaultAsync();

            builder.Append("Lịch của bạn bị trùng.\n");
            builder.Append($"Lớp: {className}\n");
            builder.Append($"Ngày học bị trùng: {date}\n");
            if (schedule!.SessionType == SessionType.FullDay)
                builder.Append($"Đã có lịch học cả ngày\n");
            else
                builder.Append($"Đã có lịch học vào buổi {schedule.SessionType.GetEnumDisplayName()}\n");
            builder.Append($"Vui lòng kiểm tra lại!");

            if (token.IsCancellationRequested)
                return;
            throw new BadRequestException(builder.ToString());
        }
    }

    private void CheckScheduleDate(DateOnly date)
    {
        // compare with Datetime +7
        var tzUtcPlus7 = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        var utcPlus7Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzUtcPlus7);
        var currentDate = DateOnly.FromDateTime(utcPlus7Time);
        var weekRange = GetWeekRange(date);

        if (currentDate > date)
            throw new BadRequestException("Bạn không thể thêm lịch trong quá khứ");
        if (weekRange.StartOfWeek.AddDays(-1) <= currentDate)
            throw new BadRequestException(
                "Bạn không thể thêm lịch. Lịch các tuần phải được thêm vào trước ngày chủ nhật của tuần trước đó.");
    }

    // ReSharper disable once UnusedMember.Local
    private (DateOnly StartOfWeek, DateOnly EndOfWeek) GetWeekRange(DateOnly date)
    {
        return GetWeekRange(new DateTime(date.Year, date.Month, date.Day));
    }

    // ReSharper disable once UnusedMember.Local
    private (DateOnly StartOfWeek, DateOnly EndOfWeek) GetNextWeekRange(DateOnly date)
    {
        return GetNextWeekRange(new DateTime(date.Year, date.Month, date.Day));
    }

    private (DateOnly StartOfWeek, DateOnly EndOfWeek) GetWeekRange(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateOnly startOfWeek = DateOnly.FromDateTime(date.AddDays(-diff).Date);
        DateOnly endOfWeek = startOfWeek.AddDays(6);

        return (startOfWeek, endOfWeek);
    }

    private (DateOnly StartOfNextWeek, DateOnly EndOfNextWeek) GetNextWeekRange(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime startOfCurrentWeek = date.AddDays(-diff).Date;

        DateOnly startOfNextWeek = DateOnly.FromDateTime(startOfCurrentWeek.AddDays(7));
        DateOnly endOfNextWeek = startOfNextWeek.AddDays(6);

        return (startOfNextWeek, endOfNextWeek);
    }

    private (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateTime date)
    {
        DateOnly startOfMonth = DateOnly.FromDateTime(new DateTime(date.Year, date.Month, 1));
        DateOnly endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return (startOfMonth, endOfMonth);
    }

    private (DateOnly StartOfMonth, DateOnly EndOfMonth) GetMonthRange(DateOnly date)
    {
        return GetMonthRange(new DateTime(date.Year, date.Month, 1));
    }
}
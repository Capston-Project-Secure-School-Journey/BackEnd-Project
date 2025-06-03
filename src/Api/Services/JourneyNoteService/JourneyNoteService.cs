using System.Collections.Concurrent;
using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.JourneyNoteService;
using Api.Extensions;
using Api.Services.DriverSchoolTripService;
using Api.Services.ParentSchoolTripService;
using Api.Services.ShuttleScheduleManagementService;
using MongoDB.Driver;

namespace Api.Services.JourneyNoteService;

public class JourneyNoteService(
    Context context,
    IShuttleScheduleManagementService shuttleScheduleManagementService,
    IParentSchoolTripService parentSchoolTripService,
    IDriverSchoolTripService driverSchoolTripService
)
    : IJourneyNoteService
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> JourneyLocks = new();

    public async Task<JourneyNote> AddJourneyNote(CreateJourneyNoteDto createJourneyNoteDto)
    {
        await IsDuplicationNote(createJourneyNoteDto);
        await parentSchoolTripService.IsManageByStudent(createJourneyNoteDto.ParentId, createJourneyNoteDto.StudentId);
        var journey = await shuttleScheduleManagementService.GetShuttleSchedule(createJourneyNoteDto.JourneyId);
        CanInsertOrUpdateNote(journey);

        var journeyNote = new JourneyNote()
        {
            Description = createJourneyNoteDto.Description,
            Type = createJourneyNoteDto.Type,
            JourneyId = createJourneyNoteDto.JourneyId,
            StudentId = createJourneyNoteDto.StudentId,
            ParentId = createJourneyNoteDto.ParentId,
            RequestedDate = DateTimeHelper.GetDateTimeUtc7(),
            IsReadByDriver = false
        };
        await context.JourneyNoteCollection.InsertOneAsync(journeyNote);
        
        var journeyLock = JourneyLocks.GetOrAdd(journey.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();
        try
        {
            await UpdateStudentTripInfoWhenChangeNote(journey, journeyNote);
            journey.IsAllNotesRead = false;
            await shuttleScheduleManagementService.UpdateShuttleSchedule(journey);
        }
        finally
        {
            journeyLock.Release();
        }
        return journeyNote;
    }

    public async Task<JourneyNote> UpdateJourneyNote(UpdateJourneyNoteDto updateJourneyNoteDto)
    {
        var journeyNote = await GetJourneyNote(updateJourneyNoteDto.JourneyNoteId);
        var journey = await shuttleScheduleManagementService.GetShuttleSchedule(journeyNote.JourneyId);
        CanInsertOrUpdateNote(journey);

        var filter = Builders<JourneyNote>.Filter.Eq(s => s.Id, journeyNote.Id);
        var update = Builders<JourneyNote>.Update
            .Set(s => s.Description, updateJourneyNoteDto.Description)
            .Set(s => s.IsReadByDriver, false);
        await context.JourneyNoteCollection.UpdateOneAsync(filter, update);
        
        var journeyLock = JourneyLocks.GetOrAdd(journey.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();
        try
        {
            journey.IsAllNotesRead = false;
            await shuttleScheduleManagementService.UpdateShuttleSchedule(journey);
        }
        finally
        {
            journeyLock.Release();
        }
        return journeyNote;
    }

    public async Task<List<JourneyNote>> GetAllJourneyNotes(Guid shuttleId)
    {
        var journeyNotes = await context.JourneyNoteCollection
            .Find(jn => jn.JourneyId == shuttleId)
            .SortByDescending(x => x.RequestedDate)
            .ToListAsync();
        return journeyNotes;
    }

    public async Task<List<JourneyNote>> GetAllJourneyNotesByParent(Guid? shuttleId, Guid parentId)
    {
        var journeyNotes = await context.JourneyNoteCollection
            .Find(jn => (shuttleId == null || jn.JourneyId == shuttleId) && jn.ParentId == parentId)
            .SortByDescending(x => x.RequestedDate)
            .ToListAsync();
        return journeyNotes;
    }

    public async Task ReadJourneyNote(Guid journeyNoteId)
    {
        var journeyNote = await GetJourneyNote(journeyNoteId);
        var journey = await shuttleScheduleManagementService.GetShuttleSchedule(journeyNote.JourneyId);

        var journeyLock = JourneyLocks.GetOrAdd(journey.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();
        try
        {
            var filter = Builders<JourneyNote>.Filter.Eq(s => s.Id, journeyNote.Id);
            var update = Builders<JourneyNote>.Update
                .Set(s => s.IsReadByDriver, true);
            await context.JourneyNoteCollection.UpdateOneAsync(filter, update);

            await UpdateJourneyIfNotesAreReady(journey);
        }
        finally
        {
            journeyLock.Release();
        }
    }

    public async Task ReadAllJourneyNote(Guid shuttleId)
    {
        var journey = await shuttleScheduleManagementService.GetShuttleSchedule(shuttleId);
        var journeyLock = JourneyLocks.GetOrAdd(shuttleId, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();
        try
        {
            var filter = Builders<JourneyNote>.Filter.Eq(s => s.JourneyId, shuttleId);
            var update = Builders<JourneyNote>.Update
                .Set(s => s.IsReadByDriver, true);
            await context.JourneyNoteCollection.UpdateManyAsync(filter, update);

            journey.IsAllNotesRead = true;
            await shuttleScheduleManagementService.UpdateShuttleSchedule(journey);
        }
        finally
        {
            journeyLock.Release();
        }
    }

    public async Task IsOwnerOfJourneyNote(Guid journeyNoteId, Guid userId, UserType userType)
    {
        bool isExist;
        if (userType == UserType.Driver)
        {
            var journeyNote = await GetJourneyNote(journeyNoteId);

            isExist = await context.ShuttleScheduleCollection
                .Find(ss => ss.Id == journeyNote.JourneyId && ss.DriverId == userId)
                .AnyAsync();
        }
        else
            isExist = await context.JourneyNoteCollection
                .Find(jn => jn.ParentId == userId && jn.Id == journeyNoteId)
                .AnyAsync();

        if (!isExist)
            throw new NotFoundException("Không tồn tại ghi chú.");
    }

    public async Task DeleteJourneyNote(Guid journeyNoteId)
    {
        var journeyNote = await GetJourneyNote(journeyNoteId);
        var journey = await shuttleScheduleManagementService.GetShuttleSchedule(journeyNote.JourneyId);
        CanDeleteNote(journey);

        var journeyLock = JourneyLocks.GetOrAdd(journey.Id, _ => new SemaphoreSlim(1, 1));
        await journeyLock.WaitAsync();
        try
        {
            var filter = Builders<JourneyNote>.Filter.Eq(s => s.Id, journeyNote.Id);
            await UpdateStudentTripInfoWhenDeleteNote(journey, journeyNote);
            await context.JourneyNoteCollection.DeleteOneAsync(filter);
            await UpdateJourneyIfNotesAreReady(journey);
        }
        finally
        {
            journeyLock.Release();
        }
    }

    private async Task<JourneyNote> GetJourneyNote(Guid journeyNoteId)
    {
        var journeyNote = await context.JourneyNoteCollection
            .Find(jn => jn.Id == journeyNoteId)
            .FirstOrDefaultAsync();

        if (journeyNote is null)
            throw new NotFoundException("Không tồn tại ghi chú.");

        return journeyNote;
    }

    private static void CanInsertOrUpdateNote(ShuttleSchedule journey)
    {
        if (journey.JourneyStatus is JourneyStatus.Cancelled or JourneyStatus.Completed)
            throw new BadRequestException("Không thêm ghi chú cho các chuyến xe đã hoàn thành.");

        if (journey.JourneyStatus is JourneyStatus.InProgress)
            throw new BadRequestException("Tài xế đã bắt đầu hành trình. Không thêm thể ghi chú.");
    }

    private static void CanDeleteNote(ShuttleSchedule journey)
    {
        if (journey.JourneyStatus is JourneyStatus.Cancelled or JourneyStatus.Completed)
            throw new BadRequestException("Không thể xóa ghi chú cho các chuyến xe đã hoàn thành.");

        if (journey.JourneyStatus is JourneyStatus.InProgress)
            throw new BadRequestException("Tài xế đã bắt đầu hành trình. Không thể xóa ghi chú.");
    }

    private async Task UpdateStudentTripInfoWhenChangeNote(ShuttleSchedule journey, JourneyNote note)
    {
        if (IsSkipPickupNoteType(note.Type))
        {
            await driverSchoolTripService.SkipStudent(journey.Id, note.StudentId, note.Type.GetEnumDisplayName());
        }
    }

    private async Task UpdateStudentTripInfoWhenDeleteNote(ShuttleSchedule journey, JourneyNote note)
    {
        if (IsSkipPickupNoteType(note.Type))
        {
            await driverSchoolTripService.UndoSkipStudent(journey.Id, note.StudentId);
        }
    }

    private async Task UpdateJourneyIfNotesAreReady(ShuttleSchedule journey)
    {
        var filter = Builders<JourneyNote>.Filter.And(
            Builders<JourneyNote>.Filter.Eq(t => t.JourneyId, journey.Id),
            Builders<JourneyNote>.Filter.Eq(t => t.IsReadByDriver, false)
        );
        var noteNotReadCount = await context.JourneyNoteCollection.CountDocumentsAsync(filter);

        if (noteNotReadCount == 0)
        {
            journey.IsAllNotesRead = true;
            await shuttleScheduleManagementService.UpdateShuttleSchedule(journey);
        }
    }

    private async Task IsDuplicationNote(CreateJourneyNoteDto journeyNoteDto)
    {
        var isDuplicate = await context.JourneyNoteCollection
            .Find(note => note.StudentId == journeyNoteDto.StudentId
                          && note.JourneyId == journeyNoteDto.JourneyId
                          && note.Type == journeyNoteDto.Type)
            .AnyAsync();
        if (isDuplicate)
            throw new BadRequestException("Đã có ghi chú loại này tồn tại.");
    }

    private static bool IsSkipPickupNoteType(JourneyNoteType journeyNoteType) =>
        journeyNoteType switch
        {
            JourneyNoteType.AbsentToday => true,
            JourneyNoteType.GoingWithParent => true,
            _ => false
        };
}
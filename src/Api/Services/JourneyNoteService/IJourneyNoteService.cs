using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.JourneyNoteService;

namespace Api.Services.JourneyNoteService;

public interface IJourneyNoteService
{
    Task<JourneyNote> AddJourneyNote(CreateJourneyNoteDto createJourneyNoteDto);
    Task<JourneyNote> UpdateJourneyNote(UpdateJourneyNoteDto updateJourneyNoteDto);
    Task<List<JourneyNote>> GetAllJourneyNotes(Guid shuttleId);
    Task<List<JourneyNote>> GetAllJourneyNotesByParent(Guid? shuttleId, Guid parentId);
    Task ReadJourneyNote(Guid journeyNoteId);
    Task ReadAllJourneyNote(Guid shuttleId);
    Task IsOwnerOfJourneyNote(Guid journeyNoteId, Guid userId, UserType userType);
    Task DeleteJourneyNote(Guid journeyNoteId);
}
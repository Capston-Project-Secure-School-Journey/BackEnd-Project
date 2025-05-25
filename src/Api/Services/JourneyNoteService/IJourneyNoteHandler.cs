using Api.Domain.Models;
using Api.DTOs.JourneyNoteService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.JourneyNoteService;

public interface IJourneyNoteHandler
{
    Task<JourneyNote> AddJourneyNote(CreateJourneyNoteDto createJourneyNoteDto);
    Task<JourneyNote> UpdateJourneyNote(UpdateJourneyNoteDto updateJourneyNoteDto, Guid parentId);
    Task<Pagination<JourneyNote>> GetAllJourneyNotes(GetJourneyNoteByDriverRequest request, Guid driverId);
    Task<Pagination<JourneyNote>> GetAllJourneyNotesByParent(GetJourneyNoteByParentRequest request, Guid parentId);
    Task DeleteJourneyNote(Guid journeyNoteId, Guid parentId);
    Task ReadJourneyNote(Guid journeyNoteId, Guid driverId);
    Task ReadAllJourneyNote(Guid shuttleId, Guid driverId);
}
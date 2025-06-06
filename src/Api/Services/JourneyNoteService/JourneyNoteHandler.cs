using Api.Common.Enums;
using Api.Common.Exceptions;
using Api.Domain.Models;
using Api.DTOs.JourneyNoteService;
using Api.Extensions;
using Api.Services.DriverSchoolTripService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.JourneyNoteService;

public class JourneyNoteHandler(
    IJourneyNoteService journeyNoteService,
    IDriverSchoolTripService driverSchoolTripService) : IJourneyNoteHandler
{
    public async Task<JourneyNote> AddJourneyNote(CreateJourneyNoteDto createJourneyNoteDto)
    {
        var journeyNote = await journeyNoteService.AddJourneyNote(createJourneyNoteDto);
        return journeyNote;
    }

    public async Task<JourneyNote> UpdateJourneyNote(UpdateJourneyNoteDto updateJourneyNoteDto, Guid parentId)
    {
        await journeyNoteService.IsOwnerOfJourneyNote(updateJourneyNoteDto.JourneyNoteId, parentId, UserType.Parent);
        var journeyNote = await journeyNoteService.UpdateJourneyNote(updateJourneyNoteDto);
        return journeyNote;
    }

    public async Task<Pagination<JourneyNote>> GetAllJourneyNotesByDriver(GetJourneyNoteRequest request, Guid driverId)
    {
        if (request.ShuttleId == null)
            throw new BadRequestException("Mã hành trình không thể trống.");
        
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(request.ShuttleId.Value, driverId);
        var journeyNotes = await journeyNoteService.GetAllJourneyNotes(request.ShuttleId.Value);
        var data = journeyNotes
            .Pagination(request.Page, request.Limit);

        var response = new Pagination<JourneyNote>(data, request.Limit, request.Page, journeyNotes.Count);
        return response;
    }

    public async Task<Pagination<JourneyNote>> GetAllJourneyNotesByParent(GetJourneyNoteRequest request,
        Guid parentId)
    {
        var journeyNotes = await journeyNoteService.GetAllJourneyNotesByParent(request.ShuttleId, parentId);
        var data = journeyNotes
            .Pagination(request.Page, request.Limit);
        
        var response = new Pagination<JourneyNote>(data, request.Limit, request.Page, journeyNotes.Count);
        return response;
    }

    public async Task DeleteJourneyNote(Guid journeyNoteId, Guid parentId)
    {
        await journeyNoteService.IsOwnerOfJourneyNote(journeyNoteId, parentId, UserType.Parent);
        await journeyNoteService.DeleteJourneyNote(journeyNoteId);
    }

    public async Task ReadJourneyNote(Guid journeyNoteId, Guid driverId)
    {
        await journeyNoteService.IsOwnerOfJourneyNote(journeyNoteId, driverId, UserType.Driver);
        await journeyNoteService.ReadJourneyNote(journeyNoteId);
    }

    public async Task ReadAllJourneyNote(Guid shuttleId, Guid driverId)
    {
        await driverSchoolTripService.IsOwnerOfShuttleSchedule(shuttleId, driverId);
        await journeyNoteService.ReadAllJourneyNote(shuttleId);
    }
}
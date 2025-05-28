using Api.Attributes;
using Api.Common.Enums;
using Api.Domain.Models;
using Api.DTOs.JourneyNoteService;
using Api.Services.JourneyNoteService;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("journey-notes")]
public class JourneyNoteController(IJourneyNoteHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    [ValidateModel]
    public async Task<JourneyNote> AddJourneyNote([FromBody] CreateJourneyNoteDto request)
    {
        var userId = this.GetUserId();
        request.ParentId = userId;
        return await handler.AddJourneyNote(request);
    }

    [HttpPut("{journeyNote}")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    [ValidateModel]
    public async Task<JourneyNote> UpdateJourneyNote([FromRoute] Guid journeyNote, [FromBody] UpdateJourneyNoteDto request)
    {
        var userId = this.GetUserId();
        request.JourneyNoteId = journeyNote;
        return await handler.UpdateJourneyNote(request, userId);
    }

    [HttpDelete("{journeyNoteId}")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task DeleteJourneyNote([FromRoute] Guid journeyNoteId)
    {
        var userId = this.GetUserId();
        await handler.DeleteJourneyNote(journeyNoteId, userId);
    }

    [HttpGet("by-driver")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task<Pagination<JourneyNote>> GetAllJourneyNotes([FromQuery] GetJourneyNoteByDriverRequest request)
    {
        var userId = this.GetUserId();
        return await handler.GetAllJourneyNotes(request, userId);
    }

    [HttpGet("by-parent")]
    [Authorize(UserType.Parent)]
    [CheckVerifiedEmail]
    public async Task<Pagination<JourneyNote>> GetAllJourneyNotesByParent(
        [FromQuery] GetJourneyNoteByParentRequest request)
    {
        var userId = this.GetUserId();
        return await handler.GetAllJourneyNotesByParent(request, userId);
    }

    [HttpPut("{journeyNoteId}/read")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task ReadJourneyNote([FromRoute] Guid journeyNoteId)
    {
        var userId = this.GetUserId();
        await handler.ReadJourneyNote(journeyNoteId, userId);
    }

    [HttpPut("mark-read")]
    [Authorize(UserType.Driver)]
    [CheckVerifiedEmail]
    public async Task ReadAllJourneyNote([FromBody] ReadAllJourneyNoteRequest request)
    {
        var userId = this.GetUserId();
        await handler.ReadAllJourneyNote(request.JourneyId, userId);
    }
}
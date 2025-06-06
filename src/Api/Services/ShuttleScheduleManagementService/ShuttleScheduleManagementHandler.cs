using Api.Domain.Models;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;
using MongoDB.Driver;

namespace Api.Services.ShuttleScheduleManagementService;

public class ShuttleScheduleManagementHandler(
    IShuttleScheduleManagementService shuttleScheduleManagement,
    IMapper mapper)
    : IShuttleScheduleManagementHandler
{
    public async Task<ShuttleScheduleView> GetShuttleScheduleView(Guid schoolId, DateOnly date)
    {
        return await shuttleScheduleManagement.GetShuttleScheduleView(schoolId, date);
    }

    public async Task<Pagination<ShuttleScheduleResponse>> GetShuttleScheduleByDate(Guid schoolId,
        GetShuttleScheduleByDateRequest request)
    {
        var query = await shuttleScheduleManagement.GetShuttleScheduleByDate(schoolId, request);
        var totalCount = await query.CountDocumentsAsync();

        var data = (await query
                .SortByDescending(x => x.SessionType)
                .ThenBy(x => x.Type)
                .ThenByDescending(x => x.NumberOfStudents)
                .Pagination(request.Page, request.Limit)
                .ToListAsync())
            .Select(mapper.Map<ShuttleScheduleResponse>);

        var response = new Pagination<ShuttleScheduleResponse>(data, request.Limit, request.Page, totalCount);

        return response;
    }

    public async Task<ShuttleSchedule> GetShuttleSchedule(Guid schoolId, Guid shuttleScheduleId)
    {
        await shuttleScheduleManagement.IsOwnerOfShuttleSchedule(schoolId, shuttleScheduleId);
        return await shuttleScheduleManagement.GetShuttleSchedule(shuttleScheduleId);
    }
}
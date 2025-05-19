using Api.Domain.Models;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class ShuttleScheduleProfile: Profile
{
    public ShuttleScheduleProfile()
    {
        CreateMap<ShuttleSchedule, ShuttleScheduleResponse>();
    }
}
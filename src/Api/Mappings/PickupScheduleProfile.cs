using Api.Domain.Models;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class PickupScheduleProfile: Profile
{
    public PickupScheduleProfile()
    {
        CreateMap<PickupSchedule, PickupScheduleResponse>();
    }
}
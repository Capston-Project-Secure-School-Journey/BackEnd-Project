using Api.Domain.Models;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class ParentSchoolTripProfile: Profile
{
    public ParentSchoolTripProfile()
    {
        CreateMap<ShuttleSchedule, ParentShuttleScheduleResponse>();
    }
}
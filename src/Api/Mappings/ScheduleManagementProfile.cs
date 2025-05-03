using Api.Domain.Models;
using Api.DTOs.ScheduleManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class ScheduleManagementProfile : Profile
{
    public ScheduleManagementProfile()
    {
        CreateMap<CreateScheduleRequest, CreateScheduleDto>();
        CreateMap<UpdateScheduleRequest, UpdateScheduleDto>();
        CreateMap<ClassSchedule, ClassScheduleResponse>();
    }
}
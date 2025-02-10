using Api.Domain.Models;
using Api.DTOs.SchoolManagement;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using AutoMapper;

namespace Api.IOC.Mappings;

public class SchoolManagementProfile : Profile
{
    public SchoolManagementProfile()
    {
        CreateMap<CreateSchoolRequest, CreateSchoolDto>();
        CreateMap<UpdateSchoolRequest, UpdateSchoolDto>();
        CreateMap<School, SchoolResponse>();
    }
}
using Api.Domain.Models;
using Api.DTOs.SchoolManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class SchoolManagementProfile : Profile
{
    public SchoolManagementProfile()
    {
        CreateMap<CreateSchoolRequest, CreateSchoolDto>();
        CreateMap<UpdateSchoolRequest, UpdateSchoolDto>();
        CreateMap<School, SchoolResponse>();
        CreateMap<School, SchoolDetailResponse>()
            .ForMember(dest => dest.Images,
                opt
                    => opt.MapFrom(src => src.Images.Select(im => im.Key).ToList()));
    }
}
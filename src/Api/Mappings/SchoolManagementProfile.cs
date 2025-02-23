using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.DTOs.SchoolManagement;
using Api.DTOs.TeacherManagement;
using Api.TransferDTOs.Responses;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
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
        
        CreateMap<CreateTeacherRequest, CreateTeacherDto>();
        CreateMap<UpdateTeacherRequest, UpdateTeacherDto>();
        CreateMap<Teacher, TeacherResponse>();
        
        CreateMap<CreateClassRequest, CreateClassDto>();
        CreateMap<UpdateClassRequest, UpdateClassDto>();
        CreateMap<Class, ClassResponse>().
            ForMember(x => x.ManagedTeachers, 
                opt => 
                    opt.MapFrom(x => x.ManagedTeachers.Select(mt => 
                        new ManagedTeacherResponse()
                        {
                            Id = mt.ManagedTeacherId, 
                            Name = string.Empty
                        })));
    }
}
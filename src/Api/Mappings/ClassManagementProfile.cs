using Api.Domain.Models;
using Api.DTOs.ClassManagement;
using Api.Extensions;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class ClassManagementProfile : Profile
{
    public ClassManagementProfile()
    {
        CreateMap<CreateClassRequest, CreateClassDto>();
        CreateMap<UpdateClassRequest, UpdateClassDto>();
        CreateMap<Class, ClassResponse>();
        CreateMap<Class, ClassDetailResponse>().ForMember(x => x.ManagedTeachers,
                opt =>
                    opt.MapFrom(x => x.ManagedTeachers.Select(mt =>
                        new ManagedTeacherResponse()
                        {
                            Id = mt.ManagedTeacherId,
                            Name = string.Empty
                        })))
            .ForMember(x => x.GradeName, opt =>
                opt.MapFrom(x => x.Grade.GetEnumDisplayName()));
    }
}
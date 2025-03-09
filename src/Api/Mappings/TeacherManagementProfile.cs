using Api.Domain.Models;
using Api.DTOs.TeacherManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class TeacherManagementProfile : Profile
{
    public TeacherManagementProfile()
    {
        CreateMap<CreateTeacherRequest, CreateTeacherDto>();
        CreateMap<UpdateTeacherRequest, UpdateTeacherDto>();
        CreateMap<Teacher, TeacherResponse>();
        CreateMap<Teacher, TeacherDetailResponse>();
    }
}
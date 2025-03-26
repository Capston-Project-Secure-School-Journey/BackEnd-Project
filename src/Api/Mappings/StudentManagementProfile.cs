using Api.Domain.Models;
using Api.DTOs.StudentManagement;
using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class StudentManagementProfile : Profile
{
    public StudentManagementProfile()
    {
        CreateMap<CreateStudentRequest, CreateStudentDto>();
        CreateMap<UpdateStudentRequest, UpdateStudentDto>();
        CreateMap<Student, StudentResponse>().ForMember(x => x.ClassName,
            opt =>
                opt.MapFrom(x => x.Class.ClassName));
        CreateMap<Student, StudentDetailResponse>().ForMember(x => x.ClassName,
            opt =>
                opt.MapFrom(x => x.Class.ClassName)).ForMember(x => x.SchoolName,
            opt =>
                opt.MapFrom(x => x.School.SchoolName));
    }
}
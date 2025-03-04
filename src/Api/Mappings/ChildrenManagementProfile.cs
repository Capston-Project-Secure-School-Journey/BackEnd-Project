using Api.Domain.Models;
using Api.DTOs.ChildrenManagement;
using AutoMapper;

namespace Api.Mappings;

public class ChildrenManagementProfile: Profile
{
    public ChildrenManagementProfile()
    {
        CreateMap<Student, ChildDto>();
        CreateMap<Student, ChildDetailDto>();
    }
}
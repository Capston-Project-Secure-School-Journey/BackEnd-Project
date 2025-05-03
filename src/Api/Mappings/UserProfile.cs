using Api.Domain.Models;
using Api.DTOs.User;
using Api.TransferDTOs.Requests;
using AutoMapper;

namespace Api.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Driver, TransferDTOs.Responses.UserProfile>().ForMember(x => x.DriverInformationImages,
                opt =>
                    opt.Ignore())
            .ForMember(x => x.VehicleImages,
                opt =>
                    opt.Ignore());
        CreateMap<User, TransferDTOs.Responses.UserProfile>();
        CreateMap<SchoolPerson, TransferDTOs.Responses.UserProfile>();
        CreateMap<UpdateProfileRequest, UpdateUserInfoDto>();
    }
}
using Api.Domain.Models;
using Api.DTOs.User;
using Api.TransferDTOs.Requests;
using AutoMapper;

namespace Api.Mappings;

public class UserProfile: Profile
{
    public UserProfile()
    {
        CreateMap<Driver, TransferDTOs.Responses.UserProfile>().ForMember(x => x.DriverInformationImage,
            opt =>
                opt.Ignore());
        CreateMap<User, TransferDTOs.Responses.UserProfile>();
        CreateMap<SchoolPerson, TransferDTOs.Responses.UserProfile>();
        CreateMap<UpdateProfileRequest, UpdateUserInfoDto>();
    }
}
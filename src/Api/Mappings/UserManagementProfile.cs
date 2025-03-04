using Api.DTOs.UserManagement;
using Api.TransferDTOs.Requests;
using AutoMapper;

namespace Api.Mappings;

public class UserManagementProfile: Profile
{
    public UserManagementProfile()
    {
        CreateMap<CreateAccountRequest, CreateUserDto>();
    }
}
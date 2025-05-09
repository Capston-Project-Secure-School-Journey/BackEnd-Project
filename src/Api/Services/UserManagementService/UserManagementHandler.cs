using Api.DTOs.UserManagement;
using Api.TransferDTOs.Requests;
using AutoMapper;

namespace Api.Services.UserManagementService;

public class UserManagementHandler(
    IUserManagement userManagement,
    IMapper mapper) : IUserManagementHandler
{
    public async Task CreateAccount(CreateAccountRequest request)
    {
        var dto = mapper.Map<CreateUserDto>(request);

        await userManagement.CreateUser(dto);
    }
}
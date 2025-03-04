using Api.DTOs.UserManagement;
using Api.TransferDTOs.Requests;
using AutoMapper;

namespace Api.Services.UserManagementService;

public class UserManagementHandler: IUserManagementHandler
{
    private readonly IUserManagement _userManagement;
    private readonly IMapper _mapper;
    public UserManagementHandler(IUserManagement userManagement,
        IMapper mapper)
    {
        _userManagement = userManagement;
        _mapper = mapper;
    }
    
    public async Task CreateAccount(CreateAccountRequest request)
    {
        var dto = _mapper.Map<CreateUserDto>(request);
        
        await _userManagement.CreateUser(dto);
    }
}
using Api.TransferDTOs.Requests;

namespace Api.Services.UserManagementService;

public interface IUserManagementHandler
{
    Task CreateAccount(CreateAccountRequest request);
}
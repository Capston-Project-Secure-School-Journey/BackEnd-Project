using Api.TransferDTOs.Requests;
using Api.TransferDTOs.Responses;

namespace Api.Services.AuthenticationService;

public interface IAuthenticationService
{
    Task<AuthenticateResponse> Login(AuthenticateRequest request);
}
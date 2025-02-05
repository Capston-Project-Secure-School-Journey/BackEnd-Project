using Api.Transfers.Requests;
using Api.Transfers.Responses;

namespace Api.Services.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<AuthenticateResponse> Login(AuthenticateRequest request);
    }
}
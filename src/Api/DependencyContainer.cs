using Api.Services.AuthenticationService;
using Api.Services.TokenService;

namespace Api.IOC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITokenService, TokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
        }
    }
}
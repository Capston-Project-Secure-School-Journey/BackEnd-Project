using Api.Attributes;
using Api.IOC.Mappings;
using Api.IOC.Services.SchoolManagement;
using Api.Services.AuthenticationService;
using Api.Services.TokenService;

namespace Api.IOC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(SchoolManagementProfile));
            services.AddSingleton<ITokenService, TokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ISchoolManagement, SchoolManagement>();
            services.AddScoped<ISchoolManagementHandler, SchoolManagementHandler>();
            services.AddSingleton<IAuthorizationChecker, AuthorizationChecker>();
        }
    }
}
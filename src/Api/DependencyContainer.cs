using Api.Attributes;
using Api.IOC.Mappings;
using Api.IOC.Services.SchoolManagement;
using Api.Services.AuthenticationService;
using Api.Services.TokenService;
using Api.Common.Utilities;
using Api.IOC.Services.UserManagementService;
using Api.Services.UploadFileService;

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
            services.AddScoped<IUserManagement, UserManagement>();
            services.AddSingleton<IAuthorizationChecker, AuthorizationChecker>();
            services.AddSingleton<IFileUploadService, S3FileUploadService>();
            
            services.AddScoped<ValidateModelAttribute>();
        }
    }
}
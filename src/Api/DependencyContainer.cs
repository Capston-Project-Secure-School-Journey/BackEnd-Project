using Api.Attributes;
using Api.Mappings;
using Api.Services.SchoolManagement;
using Api.Services.AuthenticationService;
using Api.Services.TokenService;
using Api.Common.Utilities;
using Api.Extensions;
using Api.IOC.Services.UserManagementService;
using Api.Services.ClassManagementService;
using Api.Services.StudentManagementService;
using Api.Services.TeacherManagementService;
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
            services.AddScoped<IFileUploadService, S3FileUploadService>();
            services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();
            services.AddScoped<ITeacherManagementService, TeacherManagementService>();
            services.AddScoped<ITeacherManagementHandler, TeacherManagementHandler>();
            services.AddScoped<IClassManagementService, ClassManagementService>();
            services.AddScoped<IClassManagementHandler, ClassManagementHandler>();
            services.AddScoped<IStudentManagementService, StudentManagementService>();
            services.AddScoped<IStudentManagementHandler, StudentManagementHandler>();
            
            services.AddScoped<ValidateModelAttribute>();
        }
    }
}
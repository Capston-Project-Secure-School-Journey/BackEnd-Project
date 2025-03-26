using Api.Attributes;
using Api.Services.SchoolManagement;
using Api.Services.AuthenticationService;
using Api.Services.TokenService;
using Api.Common.Utilities;
using Api.Extensions;
using Api.Services.ChildrenManagementService;
using Api.Services.UserManagementService;
using Api.Services.ClassManagementService;
using Api.Services.ScheduleManagementService;
using Api.Services.StudentManagementService;
using Api.Services.TeacherManagementService;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using Api.Services.UserService;

namespace Api;

public static class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IAuthorizationChecker, AuthorizationChecker>();
        services.AddSingleton<IVerifiedEmailChecker, VerifiedEmailChecker>();
        services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<ISchoolManagement, SchoolManagement>();
        services.AddScoped<ISchoolManagementHandler, SchoolManagementHandler>();

        services.AddScoped<IUserManagement, UserManagement>();
        services.AddScoped<IUserManagementHandler, UserManagementHandler>();

        services.AddScoped<IFileUploadService, S3FileUploadService>();

        services.AddScoped<ITeacherManagementService, TeacherManagementService>();
        services.AddScoped<ITeacherManagementHandler, TeacherManagementHandler>();

        services.AddScoped<IClassManagementService, ClassManagementService>();
        services.AddScoped<IClassManagementHandler, ClassManagementHandler>();

        services.AddScoped<IStudentManagementService, StudentManagementService>();
        services.AddScoped<IStudentManagementHandler, StudentManagementHandler>();


        services.AddScoped<IChildrenManagementService, ChildrenManagementService>();
        services.AddScoped<IChildrenManagementHandler, ChildrenManagementHandler>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserHandler, UserHandler>();

        services.AddScoped<IUserBanService, UserBanService>();

        services.AddScoped<IScheduleManagementService, ScheduleManagementService>();
        services.AddScoped<IScheduleManagementHandler, ScheduleManagementHandler>();
        services.AddSingleton<ValidateModelAttribute>();
    }
}
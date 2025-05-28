using Api.Attributes;
using Api.Services.SchoolManagement;
using Api.Services.AuthenticationService;
using Api.Services.TokenService;
using Api.Domain.ModelSettings;
using Api.Extensions;
using Api.Scheduling;
using Api.Services;
using Api.Services.ApplicationService;
using Api.Services.ApprovalProcessor;
using Api.Services.ChildrenManagementService;
using Api.Services.UserManagementService;
using Api.Services.ClassManagementService;
using Api.Services.DriverSchoolTripService;
using Api.Services.JourneyNoteService;
using Api.Services.MailService;
using Api.Services.NotificationService;
using Api.Services.ParentSchoolTripService;
using Api.Services.ScanDeviceSchoolTripService;
using Api.Services.ShuttleScheduleManagementService;
using Api.Services.ScheduleManagementService;
using Api.Services.StudentManagementService;
using Api.Services.TeacherManagementService;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using Api.Services.UserService;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
        services.AddSingleton<IStudentGroupingAlgorithm, KMeansStudentGrouping>();
        services.AddSingleton<GoogleMapsService>();
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });
        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });


        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<ISchoolManagement, SchoolManagement>();
        services.AddScoped<ISchoolManagementHandler, SchoolManagementHandler>();

        services.AddScoped<IUserManagement, UserManagement>();
        services.AddScoped<IUserManagementHandler, UserManagementHandler>();

        services.AddScoped<IFileUploadService, S3FileUploadService>();
        services.AddScoped<IUploadTransactionManager, UploadTransactionManager>();
        services.AddScoped<IFileDeleter, S3FileDeleter>();

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

        services.AddScoped<IApprovalProcessor, ApprovalProcessor>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IApplicationHandler, ApplicationHandler>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationSender, NotificationFcmSender>();

        services.AddScoped<IShuttleScheduleManagementService, ShuttleScheduleManagementService>();
        services.AddScoped<IShuttleScheduleManagementHandler, ShuttleScheduleManagementHandler>();

        services.AddScoped<IDriverSchoolTripService, DriverSchoolTripService>();
        services.AddScoped<IDriverSchoolTripHandler, DriverSchoolTripHandler>();

        services.AddScoped<IParentSchoolTripHandler, ParentSchoolTripHandler>();
        services.AddScoped<IParentSchoolTripService, ParentSchoolTripService>();

        services.AddScoped<IScanDeviceSchoolTripHandler, ScanDeviceSchoolTripHandler>();
        services.AddScoped<IScanDeviceSchoolTripService, ScanDeviceSchoolTripService>();

        services.AddScoped<IJourneyNoteHandler, JourneyNoteHandler>();
        services.AddScoped<IJourneyNoteService, JourneyNoteService>();

        services.AddScoped<IScheduleManagementService, ScheduleManagementService>();
        services.AddScoped<IScheduleManagementHandler, ScheduleManagementHandler>();
        services.AddSingleton<ValidateModelAttribute>();
        services.AddSingleton<IMailService, MailService>();
    }
}
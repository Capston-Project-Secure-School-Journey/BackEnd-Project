using System.Text.Json;
using System.Transactions;
using Api.Domain;
using Api.Domain.ModelSettings;
using Api;
using Api.DashboardAuthorizationFilters;
using Api.Hubs;
using Api.Jobs;
using Api.Pipeline.Middlewares;
using Api.Services.TokenService;
using FluentValidation;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(9, 2, 0));
var cors = "AllowSpecificOrigin";
Console.WriteLine($"connectionString: {connectionString}");
builder.Services.AddDbContext<Context>(
    options =>
    {
        options.UseMySql(connectionString, serverVersion, b => b.UseMicrosoftJson());
        // The following three options help with debugging, but should
        // be changed or removed for production.
        if (builder.Environment.IsDevelopment())
            options.LogTo(Log.Information, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
    });

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseStorage(
            new MySqlStorage(
                builder.Configuration.GetConnectionString("HangfireConnection"),
                new MySqlStorageOptions
                {
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "Hangfire"
                }))
);
builder.Services.AddHangfireServer();

DependencyContainer.RegisterServices(builder.Services);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = TokenService.GetTokenValidationParameters(tokenSettings!);
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    context.Token = authHeader.StartsWith("Bearer ") ? authHeader[7..] : authHeader;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "";
        document.Info.Description = "";
        document.Info.TermsOfService = "None";
        document.Info.Contact = new OpenApiContact
        {
            Name = "SSAST Api",
            Email = string.Empty,
            Url = ""
        };
    };
    config.AddSecurity("Bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Authorization",
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: {your JWT token}."
    });

    config.OperationProcessors.Add(
        new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});


builder.Services.AddValidatorsFromAssemblyContaining<Program>();


builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use snake_case for both requests and responses
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.Configure<ApiBehaviorOptions>(options
    => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy(cors,
        // d => d.WithOrigins(clientPath!)
        d => d.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});


builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.UseHttpClientMetrics();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.MySQL(
        connectionString: connectionString,
        tableName: "error_logs",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();

builder.Services.AddSerilog();
MongoMappingConfig.RegisterMappings();

var app = builder.Build();

app.UsePathBase("/api");
app.UseAuthentication();
app.UseMiddleware<TimeMeasuringMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMetricServer();
app.UseHttpMetrics();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseCors(cors);
app.MapControllers();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization =
    [
        new DashboardAuthorizationFilter()
    ]
});
app.MapHub<TripHub>("/trip-hub");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any()) await dbContext.Database.MigrateAsync();

    if (!await dbContext.Users.AnyAsync()) await DbInitializer.SeedData(dbContext);
}

JobRegister.Register();
await app.RunAsync();
await Log.CloseAndFlushAsync();

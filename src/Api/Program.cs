using System.Text.Json;
using Api.Domain;
using Api.Domain.ModelSettings;
using Api;
using Api.Pipeline.Middlewares;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(9, 2, 0));
var cors = "AllowSpecificOrigin";
Console.WriteLine($"connectionString: {connectionString}");
builder.Services.AddDbContext<Context>(
    options => options.UseMySql(connectionString, serverVersion,  b => b.UseMicrosoftJson())
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());

DependencyContainer.RegisterServices(builder.Services);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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


builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use snake_case for both requests and responses
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options
    => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy(cors,
        d => d.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});


builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseOpenApi();
app.UseSwaggerUI();
app.UseCors(cors);
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }

    if (!dbContext.Users.Any())
    {
        DbInitializer.SeedData(dbContext);
    }
}

app.Run();





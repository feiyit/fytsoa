using System.Text.Json.Serialization;
using FytSoa.ApiService;
using FytSoa.ApiService.Filters;
using FytSoa.ApiService.Swagger;
using FytSoa.Application;
using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;
using FytSoa.CrossCutting;
using FytSoa.Sugar;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
// SignalR
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "FytSoaCors",
        policy  =>
        {
            policy.WithOrigins("http://localhost:2318","http://localhost:2319")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders ("X-Refresh-Token");
        });
    /*options.AddPolicy(name: "FytSoaCors",
        policy  =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .WithExposedHeaders ("X-Refresh-Token");
        });*/
});

// Add services to the container.
AppUtils.Init(builder.WebHost);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AopActionFilter>();
    options.Filters.Add<GlobalExceptionFilter>(); 
    options.Filters.Add<UnitOfWorkFilter>();
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.AllowTrailingCommas = false;
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new LongJsonConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// Rate limiting policies (login, etc.)
builder.Services.AddFytRateLimiting(builder.Configuration);

// Register DI
builder.Services.RegisterServices();

// Mapper
builder.Services.AddMapperProfile();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseSwaggerSetup();
}
app.UseSwaggerSetup();
//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "upload")),
    RequestPath = "/upload",
});

app.UseSetup();

app.MapControllers().RequireAuthorization();
app.MapHub<ChatHub>("/chathub");

app.Run();

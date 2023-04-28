using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using FytSoa.Application;
using FytSoa.Common.Extensions;
using FytSoa.Common.Utils;
using FytSoa.CrossCutting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});
builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddRazorPages().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.AllowTrailingCommas = false;
    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new LongJsonConverter());
});

AppUtils.Init(builder.WebHost);
// Register DI
builder.Services.RegisterServices();
// Mapper
builder.Services.AddMapperProfile();

builder.Services.AddControllers();
builder.Services.AddRazorPages(options => {
    options.Conventions.AuthorizeFolder("/User");
    options.Conventions.AllowAnonymousToPage("/User/Login");
    options.Conventions.AllowAnonymousToPage("/User/Register");
    options.Conventions.AllowAnonymousToPage("/User/SignOut");
});
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
{
    option.LoginPath = "/user/login";
    option.AccessDeniedPath = "/user/login";
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
//AppUtils.ServiceProvider = app.Services;

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.AppSetup();

app.UseRouting();

app.UseAuthentication(); 

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});

app.Run();
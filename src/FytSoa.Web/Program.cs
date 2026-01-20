using System.Text.Encodings.Web;
using System.Text.Unicode;
using FytSoa.Application;
using FytSoa.Common.Utils;
using FytSoa.CrossCutting;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);
AppUtils.Init(builder.WebHost);
builder.Services.RegisterServices(false);
// Mapper
builder.Services.AddMapperProfile();
builder.Services.Configure<WebEncoderOptions>(options =>options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseStatusCodePagesWithRedirects("/error?statusCode={0}");
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.AppSetup();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
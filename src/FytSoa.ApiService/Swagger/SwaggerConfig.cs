using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using FytApi.MUI;
using Microsoft.OpenApi.Models;

namespace FytSoa.ApiService.Swagger;

public static class SwaggerConfig
{
    private static readonly List<SwaggerVersion> SwaggerVersions = new()
    {
        new SwaggerVersion {Version="v1",Title = "系统",Code = "Sys"},
        new SwaggerVersion {Version="v2",Title = "内容",Code = "Cms"},
        new SwaggerVersion {Version="v3",Title = "资产",Code = "Am"},
        new SwaggerVersion {Version="v4",Title = "Flow",Code = "Flow"}
    };
    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddSwaggerGen(s =>
        {
            foreach (var item in SwaggerVersions)
            {
                s.SwaggerDoc(item.Version, new OpenApiInfo
                {
                    Version = item.Version,
                    Title = item.Title,
                });
            }
            s.OrderActionsBy(o => o.RelativePath);

            //Add Xml
            //s.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "FytSoa.Domain.xml"),true);
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            foreach (var fi in dir.EnumerateFiles("*.xml"))
            {
                var doc = XDocument.Load(fi.FullName);
                s.IncludeXmlComments(() => new XPathDocument(doc.CreateReader()), true);
                //s.SchemaFilter<DescribeEnumMembers>(doc);
            }
            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "accessToken",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                         },
                         new string[] {}
                     }
            });

        });
    }

    public static void UseSwaggerSetup(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            foreach (var item in SwaggerVersions)
            {
                c.SwaggerEndpoint("/swagger/" + item.Version + "/swagger.json", item.Title + "-" + item.Code);
            }
        });
        app.UseFytApiUI(c =>
        {
            foreach (var item in SwaggerVersions)
            {
                c.SwaggerEndpoint("/swagger/" + item.Version + "/swagger.json", item.Title, item.Code);
            }
        });

    }
}
using System.Text;
using FytSoa.Common.Utils;
using FytSoa.Common.Jwt.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FytSoa.CrossCutting;

public static  class JwtStrapperIoC
{
    public static void AddJwtConfiguration (this IServiceCollection services) {
            services.Configure<JwtModel> (AppUtils.Configuration.GetSection ("JwtAuth"));
            var token = AppUtils.Configuration.GetSection ("JwtAuth").Get<JwtModel> ()??new JwtModel();

            services.AddAuthentication (x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer (x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters {
                    IssuerSigningKey = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (token.Security)),
                        ValidateLifetime = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        ValidIssuer = token.Issuer,
                        ValidAudience = token.Audience,
                        /*AudienceValidator = (m, n, z) => 
                            m != null && m.FirstOrDefault()!.Equals(JwtConst.ValidAudience),*/
                };
                x.Events = new JwtBearerEvents () {
                    OnMessageReceived = context => {
                        var values = context.Request.Headers["accessToken"];
                        context.Token = values.FirstOrDefault ();
                        return Task.CompletedTask;
                    },
                };
            });

            services.AddAuthorization (options => {
                options.AddPolicy ("App", policy => policy.RequireRole ("App").Build ());
                options.AddPolicy ("Admin", policy => policy.RequireRole ("Admin").Build ());
            });
        }
}
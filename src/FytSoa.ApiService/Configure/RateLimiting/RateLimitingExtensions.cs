using System.Text.Json;
using System.Threading.RateLimiting;
using FytSoa.Common.Result;
using Microsoft.AspNetCore.RateLimiting;

namespace FytSoa.ApiService;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Central place to register rate limiting policies for the API service.
    /// Add new policies here and reference them via [EnableRateLimiting("policy-name")].
    /// </summary>
    public static IServiceCollection AddFytRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                if (context.HttpContext.Response.HasStarted) return;

                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                var payload = new ApiResult<string?>
                {
                    Code = StatusCodes.Status429TooManyRequests,
                    Message = "请求过于频繁，请稍后再试"
                };
                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(payload), token);
            };

            // Login: per-IP fixed window limiter.
            options.AddPolicy("login", httpContext =>
            {
                var permitLimit = configuration.GetValue("RateLimiting:Login:PermitLimit", 5);
                var windowSeconds = configuration.GetValue("RateLimiting:Login:WindowSeconds", 60);

                // NOTE: For reverse proxy deployments, consider enabling forwarded headers and using X-Forwarded-For.
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}


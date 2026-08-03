using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations;

public static class RateLimitingConfiguration
{
    public const string AdminLoginPolicy = "AdminLogin";
    public const string PatientLoginPolicy = "PatientLogin";
    public const string AppointmentBookingPolicy = "AppointmentBooking";
    public const string GeneralPolicy = "General";

    public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            AddIpPolicy(
                options,
                configuration,
                AdminLoginPolicy);

            AddIpPolicy(
                options,
                configuration,
                PatientLoginPolicy);

            AddUserPolicy(
                options,
                configuration,
                AppointmentBookingPolicy);

            AddUserOrIpPolicy(
                options,
                configuration,
                GeneralPolicy);

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiting");

                logger.LogWarning(
                    "Solicitud rechazada por Rate Limiting. Path: {Path}, IP: {IpAddress}",
                    context.HttpContext.Request.Path,
                    context.HttpContext.Connection.RemoteIpAddress);

                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                context.HttpContext.Response.ContentType =
                    "application/json";

                var error = new ErrorResponse(
                    "RATE_LIMIT_EXCEEDED",
                    "Se excedió el límite de solicitudes permitido.");

                await context.HttpContext.Response.WriteAsJsonAsync(
                    error,
                    cancellationToken);
            };
        });

        return services;
    }

    private static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        IConfiguration configuration,
        string policyName)
    {
        var permitLimit = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:PermitLimit");

        var windowSeconds = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:WindowSeconds");

        options.AddFixedWindowLimiter(
            policyName,
            limiterOptions =>
            {
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window =
                    TimeSpan.FromSeconds(windowSeconds);

                limiterOptions.QueueLimit = 0;
                limiterOptions.QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst;
            });
    }
    private static void AddIpPolicy(
    RateLimiterOptions options,
    IConfiguration configuration,
    string policyName)
    {
        var permitLimit = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:PermitLimit");

        var windowSeconds = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:WindowSeconds");

        options.AddPolicy(
            policyName,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    factory: _ => CreateOptions(
                        permitLimit,
                        windowSeconds)));
    }

    private static void AddUserPolicy(
        RateLimiterOptions options,
        IConfiguration configuration,
        string policyName)
    {
        var permitLimit = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:PermitLimit");

        var windowSeconds = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:WindowSeconds");

        options.AddPolicy(
            policyName,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.User.Identity?.Name
                        ?? "anonymous",
                    factory: _ => CreateOptions(
                        permitLimit,
                        windowSeconds)));
    }

    private static void AddUserOrIpPolicy(
        RateLimiterOptions options,
        IConfiguration configuration,
        string policyName)
    {
        var permitLimit = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:PermitLimit");

        var windowSeconds = configuration.GetValue<int>(
            $"RateLimiting:{policyName}:WindowSeconds");

        options.AddPolicy(
            policyName,
            httpContext =>
            {
                var partitionKey =
                    httpContext.User.Identity?.IsAuthenticated == true
                        ? httpContext.User.Identity.Name ?? "authenticated"
                        : httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => CreateOptions(
                        permitLimit,
                        windowSeconds));
            });
    }

    private static FixedWindowRateLimiterOptions CreateOptions(
        int permitLimit,
        int windowSeconds)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = 0,
            QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst
        };
    }
}
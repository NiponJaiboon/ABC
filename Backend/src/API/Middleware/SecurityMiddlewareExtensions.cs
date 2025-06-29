using System.Threading.RateLimiting;
using Infrastructure.Services;

namespace API.Middleware;

// Step 13: Security Middleware Configuration
public static class SecurityMiddlewareExtensions
{
    private const string AnonymousPartitionKey = "anonymous";

    public static IServiceCollection AddSecurityMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Rate Limiting
        services.AddRateLimiter(options =>
        {
            // Global rate limiting policy
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? AnonymousPartitionKey,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100, // 100 requests
                        Window = TimeSpan.FromMinutes(1) // per minute
                    }));

            // API rate limiting policy - more restrictive
            options.AddPolicy("ApiPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? AnonymousPartitionKey,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 60, // 60 requests
                        Window = TimeSpan.FromMinutes(1) // per minute
                    }));

            // Authentication rate limiting policy - very restrictive
            options.AddPolicy("AuthPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? AnonymousPartitionKey,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10, // 10 requests
                        Window = TimeSpan.FromMinutes(1) // per minute
                    }));

            // External auth rate limiting - moderate
            options.AddPolicy("ExternalAuthPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? AnonymousPartitionKey,
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20, // 20 requests
                        Window = TimeSpan.FromMinutes(1) // per minute
                    }));

            // Handle rate limit exceeded
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429; // Too Many Requests
                await context.HttpContext.Response.WriteAsync(
                    "Rate limit exceeded. Please try again later.",
                    cancellationToken: token);
            };
        });

        // Configure CORS
        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "https://localhost:3000" };

            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains();
            });

            // More restrictive policy for production
            options.AddPolicy("RestrictiveCorsPolicy", policy =>
            {
                policy.WithOrigins("https://yourdomain.com")
                      .WithMethods("GET", "POST", "PUT", "DELETE")
                      .WithHeaders("Content-Type", "Authorization")
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        // Use rate limiting
        app.UseRateLimiter();

        // Use CORS
        var corsPolicy = app.Environment.IsDevelopment() ? "DefaultCorsPolicy" : "RestrictiveCorsPolicy";
        app.UseCors(corsPolicy);

        // Security headers middleware
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}

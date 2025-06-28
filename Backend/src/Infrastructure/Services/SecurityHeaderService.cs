using Core.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface ISecurityHeaderService
{
    void ConfigureSecurityHeaders(IApplicationBuilder app);
    Task ApplySecurityHeadersAsync(HttpContext context);
}

public class SecurityHeaderService : ISecurityHeaderService
{
    private readonly ILogger<SecurityHeaderService> _logger;

    public SecurityHeaderService(ILogger<SecurityHeaderService> logger)
    {
        _logger = logger;
    }

    public void ConfigureSecurityHeaders(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            await ApplySecurityHeadersAsync(context);
            await next();
        });
    }

    public async Task ApplySecurityHeadersAsync(HttpContext context)
    {
        try
        {
            var response = context.Response;

            // Content Security Policy
            if (!response.Headers.ContainsKey("Content-Security-Policy"))
            {
                response.Headers["Content-Security-Policy"] = SecurityConstants.SecurityHeaders.ContentSecurityPolicy;
            }

            // X-Content-Type-Options
            if (!response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                response.Headers["X-Content-Type-Options"] = "nosniff";
            }

            // X-Frame-Options
            if (!response.Headers.ContainsKey("X-Frame-Options"))
            {
                response.Headers["X-Frame-Options"] = "DENY";
            }

            // X-XSS-Protection
            if (!response.Headers.ContainsKey("X-XSS-Protection"))
            {
                response.Headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Referrer-Policy
            if (!response.Headers.ContainsKey("Referrer-Policy"))
            {
                response.Headers["Referrer-Policy"] = SecurityConstants.SecurityHeaders.ReferrerPolicy;
            }

            // Permissions-Policy
            if (!response.Headers.ContainsKey("Permissions-Policy"))
            {
                response.Headers["Permissions-Policy"] = SecurityConstants.SecurityHeaders.PermissionsPolicy;
            }

            // Strict-Transport-Security (only for HTTPS)
            if (context.Request.IsHttps && !response.Headers.ContainsKey("Strict-Transport-Security"))
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }

            // Remove server header for security
            if (response.Headers.ContainsKey("Server"))
            {
                response.Headers.Remove("Server");
            }

            // Remove X-Powered-By header
            if (response.Headers.ContainsKey("X-Powered-By"))
            {
                response.Headers.Remove("X-Powered-By");
            }

            // Cache control for sensitive endpoints
            if (IsSensitiveEndpoint(context.Request.Path))
            {
                response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                response.Headers["Pragma"] = "no-cache";
                response.Headers["Expires"] = "0";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying security headers");
        }

        await Task.CompletedTask;
    }

    private static bool IsSensitiveEndpoint(PathString path)
    {
        var sensitiveEndpoints = new[]
        {
            "/api/account",
            "/api/auth",
            "/connect/token",
            "/connect/userinfo"
        };

        return sensitiveEndpoints.Any(endpoint =>
            path.Value?.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}

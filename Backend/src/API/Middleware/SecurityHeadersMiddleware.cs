namespace API.Middleware;

// Step 13: Security Headers Middleware
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Apply security headers before processing the request
        ApplySecurityHeaders(context);

        // Log suspicious activities
        await LogSuspiciousActivity(context);

        await _next(context);
    }

    private static void ApplySecurityHeaders(HttpContext context)
    {
        var response = context.Response;

        // Prevent page from being displayed in a frame (Clickjacking protection)
        if (!response.Headers.ContainsKey("X-Frame-Options"))
        {
            response.Headers["X-Frame-Options"] = "DENY";
        }

        // Enable XSS filtering
        if (!response.Headers.ContainsKey("X-XSS-Protection"))
        {
            response.Headers["X-XSS-Protection"] = "1; mode=block";
        }

        // Prevent MIME type sniffing
        if (!response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            response.Headers["X-Content-Type-Options"] = "nosniff";
        }

        // Content Security Policy - Restrictive for security
        if (!response.Headers.ContainsKey("Content-Security-Policy"))
        {
            response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "object-src 'none'; " +
                "base-uri 'self';";
        }

        // Strict Transport Security (HSTS) - Only for HTTPS
        if (context.Request.IsHttps && !response.Headers.ContainsKey("Strict-Transport-Security"))
        {
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Referrer Policy
        if (!response.Headers.ContainsKey("Referrer-Policy"))
        {
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }

        // Permissions Policy (Feature Policy)
        if (!response.Headers.ContainsKey("Permissions-Policy"))
        {
            response.Headers["Permissions-Policy"] =
                "geolocation=(), " +
                "microphone=(), " +
                "camera=(), " +
                "usb=(), " +
                "bluetooth=(), " +
                "payment=()";
        }

        // Cross-Origin Policies
        if (!response.Headers.ContainsKey("Cross-Origin-Embedder-Policy"))
        {
            response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        }

        if (!response.Headers.ContainsKey("Cross-Origin-Opener-Policy"))
        {
            response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        }

        if (!response.Headers.ContainsKey("Cross-Origin-Resource-Policy"))
        {
            response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
        }

        // Remove server identification headers
        response.Headers.Remove("Server");
        response.Headers.Remove("X-Powered-By");
        response.Headers.Remove("X-AspNet-Version");
        response.Headers.Remove("X-AspNetMvc-Version");
    }

    private async Task LogSuspiciousActivity(HttpContext context)
    {
        var request = context.Request;

        // Log potentially suspicious activities
        var suspiciousPatterns = new[]
        {
            "script", "javascript:", "vbscript:", "onload", "onerror",
            "../", "..\\", "union", "select", "drop", "insert", "update", "delete",
            "<script", "</script", "eval(", "setTimeout(", "setInterval("
        };

        var requestPath = request.Path.Value?.ToLowerInvariant() ?? "";
        var queryString = request.QueryString.Value?.ToLowerInvariant() ?? "";
        var userAgent = request.Headers["User-Agent"].ToString().ToLowerInvariant();

        foreach (var pattern in suspiciousPatterns)
        {
            if (requestPath.Contains(pattern) || queryString.Contains(pattern))
            {
                _logger.LogWarning("Suspicious request detected from {RemoteIpAddress}: Path={Path}, Query={Query}, UserAgent={UserAgent}",
                    context.Connection.RemoteIpAddress,
                    request.Path,
                    request.QueryString,
                    userAgent);
                break;
            }
        }

        // Log requests with missing or suspicious User-Agent
        if (string.IsNullOrEmpty(userAgent) || userAgent.Length < 10)
        {
            _logger.LogWarning("Request with suspicious User-Agent from {RemoteIpAddress}: {UserAgent}",
                context.Connection.RemoteIpAddress,
                userAgent);
        }

        await Task.CompletedTask;
    }
}

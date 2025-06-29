namespace API.Middleware;

// Step 13: Security Headers Service Interface
public interface ISecurityHeaderService
{
    Task ApplySecurityHeadersAsync(HttpContext context);
}

// Step 13: Security Headers Service Implementation
public class SecurityHeaderService : ISecurityHeaderService
{
    private const string DefaultContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https:; " +
        "connect-src 'self' https:; " +
        "frame-ancestors 'none';";

    public Task ApplySecurityHeadersAsync(HttpContext context)
    {
        var response = context.Response;

        // Prevent page from being displayed in a frame (Clickjacking protection)
        response.Headers["X-Frame-Options"] = "DENY";

        // Enable XSS filtering
        response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Prevent MIME type sniffing
        response.Headers["X-Content-Type-Options"] = "nosniff";

        // Content Security Policy
        response.Headers["Content-Security-Policy"] = DefaultContentSecurityPolicy;

        // Strict Transport Security (HSTS)
        if (context.Request.IsHttps)
        {
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Referrer Policy
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions Policy (Feature Policy)
        response.Headers["Permissions-Policy"] =
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "usb=(), " +
            "bluetooth=(), " +
            "payment=()";

        // Cross-Origin Embedder Policy
        response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";

        // Cross-Origin Opener Policy
        response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";

        // Cross-Origin Resource Policy
        response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // Remove server identification
        response.Headers.Remove("Server");
        response.Headers.Remove("X-Powered-By");
        response.Headers.Remove("X-AspNet-Version");
        response.Headers.Remove("X-AspNetMvc-Version");

        return Task.CompletedTask;
    }
}

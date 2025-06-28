using Core.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IAuthenticationAuditService
{
    Task LogAuthenticationAttemptAsync(string userId, string method, string provider, bool success, string? ipAddress = null, string? userAgent = null);
    Task LogExternalLoginAttemptAsync(string provider, string email, bool success, bool isNewUser, string? ipAddress = null);
    Task LogAccountLinkingAsync(string userId, string provider, bool success, string action = "link");
    Task LogPasswordChangeAsync(string userId, bool success, string? ipAddress = null);
    Task LogSecurityEventAsync(string userId, string eventType, string description, string? ipAddress = null);
}

public class AuthenticationAuditService : IAuthenticationAuditService
{
    private readonly ILogger<AuthenticationAuditService> _logger;

    public AuthenticationAuditService(ILogger<AuthenticationAuditService> logger)
    {
        _logger = logger;
    }

    public async Task LogAuthenticationAttemptAsync(string userId, string method, string provider, bool success, string? ipAddress = null, string? userAgent = null)
    {
        var logMessage = "Authentication attempt: UserId={UserId}, Method={Method}, Provider={Provider}, Success={Success}, IP={IpAddress}, UserAgent={UserAgent}";

        if (success)
        {
            _logger.LogInformation(logMessage, userId, method, provider, success, ipAddress, userAgent);
        }
        else
        {
            _logger.LogWarning(logMessage, userId, method, provider, success, ipAddress, userAgent);
        }

        // In a real implementation, you would also save to database
        await Task.CompletedTask;
    }

    public async Task LogExternalLoginAttemptAsync(string provider, string email, bool success, bool isNewUser, string? ipAddress = null)
    {
        var logMessage = "External login attempt: Provider={Provider}, Email={Email}, Success={Success}, IsNewUser={IsNewUser}, IP={IpAddress}";

        if (success)
        {
            _logger.LogInformation(logMessage, provider, email, success, isNewUser, ipAddress);
        }
        else
        {
            _logger.LogWarning(logMessage, provider, email, success, isNewUser, ipAddress);
        }

        await Task.CompletedTask;
    }

    public async Task LogAccountLinkingAsync(string userId, string provider, bool success, string action = "link")
    {
        var logMessage = "Account linking event: UserId={UserId}, Provider={Provider}, Action={Action}, Success={Success}";

        if (success)
        {
            _logger.LogInformation(logMessage, userId, provider, action, success);
        }
        else
        {
            _logger.LogWarning(logMessage, userId, provider, action, success);
        }

        await Task.CompletedTask;
    }

    public async Task LogPasswordChangeAsync(string userId, bool success, string? ipAddress = null)
    {
        var logMessage = "Password change attempt: UserId={UserId}, Success={Success}, IP={IpAddress}";

        if (success)
        {
            _logger.LogInformation(logMessage, userId, success, ipAddress);
        }
        else
        {
            _logger.LogWarning(logMessage, userId, success, ipAddress);
        }

        await Task.CompletedTask;
    }

    public async Task LogSecurityEventAsync(string userId, string eventType, string description, string? ipAddress = null)
    {
        _logger.LogWarning("Security event: UserId={UserId}, EventType={EventType}, Description={Description}, IP={IpAddress}",
            userId, eventType, description, ipAddress);

        await Task.CompletedTask;
    }
}

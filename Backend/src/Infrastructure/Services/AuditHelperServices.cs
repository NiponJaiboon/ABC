using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Helper class for user activity details
/// </summary>
public class UserActivityDetails
{
    public string? ResourceId { get; set; }
    public string? Description { get; set; }
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
}

/// <summary>
/// Helper service for extracting audit information from HTTP context
/// </summary>
public static class AuditContextHelper
{
    /// <summary>
    /// Extract IP address from HTTP context
    /// </summary>
    public static string GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null)
            return "Unknown";

        // Check for X-Forwarded-For header (load balancer/proxy)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP from the chain
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header (nginx proxy)
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Extract user agent from HTTP context
    /// </summary>
    public static string? GetUserAgent(HttpContext? httpContext)
    {
        return httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    /// <summary>
    /// Create authentication audit request for login success
    /// </summary>
    public static AuthenticationAuditRequest CreateLoginSuccessRequest(
        string? userId, string? username, string? email, string authMethod,
        string? sessionId, HttpContext? httpContext, string? additionalData = null)
    {
        return new AuthenticationAuditRequest
        {
            UserId = userId,
            Username = username,
            Email = email,
            EventType = AuditEventTypes.Login,
            Result = AuthenticationResult.Success,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = GetUserAgent(httpContext),
            AuthenticationMethod = authMethod,
            SessionId = sessionId,
            AdditionalData = additionalData
        };
    }

    /// <summary>
    /// Create authentication audit request for login failure
    /// </summary>
    public static AuthenticationAuditRequest CreateLoginFailureRequest(
        string? userId, string? username, string? email, string authMethod,
        string failureReason, HttpContext? httpContext, string? additionalData = null)
    {
        return new AuthenticationAuditRequest
        {
            UserId = userId,
            Username = username,
            Email = email,
            EventType = AuditEventTypes.Login,
            Result = AuthenticationResult.Failed,
            FailureReason = failureReason,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = GetUserAgent(httpContext),
            AuthenticationMethod = authMethod,
            AdditionalData = additionalData
        };
    }

    /// <summary>
    /// Create failed login attempt request
    /// </summary>
    public static FailedLoginAttemptRequest CreateFailedLoginAttemptRequest(
        string? userId, string? username, string? email, string failureReason,
        HttpContext? httpContext, string? additionalData = null)
    {
        return new FailedLoginAttemptRequest
        {
            UserId = userId,
            Username = username,
            Email = email,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = GetUserAgent(httpContext),
            FailureReason = failureReason,
            AdditionalData = additionalData
        };
    }

    /// <summary>
    /// Create user activity audit request
    /// </summary>
    public static UserActivityAuditRequest CreateUserActivityRequest(
        string userId, string username, string action, string resource,
        HttpContext? httpContext, UserActivityDetails? details = null)
    {
        return new UserActivityAuditRequest
        {
            UserId = userId,
            Username = username,
            Action = action,
            Resource = resource,
            ResourceId = details?.ResourceId,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = GetUserAgent(httpContext),
            Details = details?.Description,
            OldValues = details?.OldValues,
            NewValues = details?.NewValues
        };
    }

    /// <summary>
    /// Create security audit request
    /// </summary>
    public static SecurityAuditRequest CreateSecurityAuditRequest(
        string? userId, string eventType, SecuritySeverity severity,
        string? description, HttpContext? httpContext, object? additionalData = null)
    {
        return new SecurityAuditRequest
        {
            UserId = userId,
            EventType = eventType,
            Severity = severity,
            IpAddress = GetClientIpAddress(httpContext),
            UserAgent = GetUserAgent(httpContext),
            RequestPath = httpContext?.Request.Path.Value,
            Description = description,
            AdditionalData = additionalData
        };
    }
}

/// <summary>
/// Composite audit service that coordinates all audit operations
/// </summary>
public class CompositeAuditService
{
    private readonly IAuthenticationAuditService _authAuditService;
    private readonly IFailedLoginTrackingService _failedLoginService;
    private readonly IUserActivityAuditService _activityAuditService;
    private readonly ISecurityAuditService _securityAuditService;

    public CompositeAuditService(
        IAuthenticationAuditService authAuditService,
        IFailedLoginTrackingService failedLoginService,
        IUserActivityAuditService activityAuditService,
        ISecurityAuditService securityAuditService)
    {
        _authAuditService = authAuditService;
        _failedLoginService = failedLoginService;
        _activityAuditService = activityAuditService;
        _securityAuditService = securityAuditService;
    }

    /// <summary>
    /// Log a successful login and related activities
    /// </summary>
    public async Task LogSuccessfulLoginAsync(
        string? userId, string? username, string? email, string authMethod,
        string? sessionId, HttpContext? httpContext, string? additionalData = null)
    {
        // Log authentication event
        var authRequest = AuditContextHelper.CreateLoginSuccessRequest(
            userId, username, email, authMethod, sessionId, httpContext, additionalData);
        await _authAuditService.LogAuthenticationEventAsync(authRequest);

        // Log user activity
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
        {
            var activityRequest = AuditContextHelper.CreateUserActivityRequest(
                userId, username, AuditEventTypes.Login, AuditResourceTypes.Session,
                httpContext, new UserActivityDetails
                {
                    ResourceId = sessionId,
                    Description = $"Successful login using {authMethod}"
                });
            await _activityAuditService.LogActivityAsync(activityRequest);
        }
    }

    /// <summary>
    /// Log a failed login and track the failure
    /// </summary>
    public async Task LogFailedLoginAsync(
        string? userId, string? username, string? email, string authMethod,
        string failureReason, HttpContext? httpContext, string? additionalData = null)
    {
        // Log authentication event
        var authRequest = AuditContextHelper.CreateLoginFailureRequest(
            userId, username, email, authMethod, failureReason, httpContext, additionalData);
        await _authAuditService.LogAuthenticationEventAsync(authRequest);

        // Track failed login attempt
        var failedRequest = AuditContextHelper.CreateFailedLoginAttemptRequest(
            userId, username, email, failureReason, httpContext, additionalData);
        await _failedLoginService.RecordFailedAttemptAsync(failedRequest);

        // Log security event if suspicious
        var ipAddress = AuditContextHelper.GetClientIpAddress(httpContext);
        var recentFailures = await _failedLoginService.GetFailedAttemptsFromIpAsync(
            ipAddress, TimeSpan.FromMinutes(15));

        if (recentFailures.Count() >= 3)
        {
            var securityRequest = AuditContextHelper.CreateSecurityAuditRequest(
                userId, AuditEventTypes.SuspiciousActivity, SecuritySeverity.Medium,
                $"Multiple failed login attempts from IP {ipAddress}", httpContext,
                new { FailureCount = recentFailures.Count(), TimeWindow = "15 minutes" });
            await _securityAuditService.LogSecurityEventAsync(securityRequest);
        }
    }

    /// <summary>
    /// Log a logout event
    /// </summary>
    public async Task LogLogoutAsync(
        string userId, string username, string? sessionId, HttpContext? httpContext)
    {
        // Log authentication event
        var authRequest = new AuthenticationAuditRequest
        {
            UserId = userId,
            Username = username,
            EventType = AuditEventTypes.Logout,
            Result = AuthenticationResult.Success,
            IpAddress = AuditContextHelper.GetClientIpAddress(httpContext),
            UserAgent = AuditContextHelper.GetUserAgent(httpContext),
            SessionId = sessionId
        };
        await _authAuditService.LogAuthenticationEventAsync(authRequest);

        // Log user activity
        var activityRequest = AuditContextHelper.CreateUserActivityRequest(
            userId, username, AuditEventTypes.Logout, AuditResourceTypes.Session,
            httpContext, new UserActivityDetails
            {
                ResourceId = sessionId,
                Description = "User logout"
            });
        await _activityAuditService.LogActivityAsync(activityRequest);
    }

    /// <summary>
    /// Log a registration event
    /// </summary>
    public async Task LogRegistrationAsync(
        string userId, string username, string email, bool success,
        string? failureReason, HttpContext? httpContext)
    {
        var authRequest = new AuthenticationAuditRequest
        {
            UserId = userId,
            Username = username,
            Email = email,
            EventType = AuditEventTypes.Registration,
            Result = success ? AuthenticationResult.Success : AuthenticationResult.Failed,
            FailureReason = failureReason,
            IpAddress = AuditContextHelper.GetClientIpAddress(httpContext),
            UserAgent = AuditContextHelper.GetUserAgent(httpContext),
            AuthenticationMethod = "LOCAL"
        };
        await _authAuditService.LogAuthenticationEventAsync(authRequest);

        if (success)
        {
            var activityRequest = AuditContextHelper.CreateUserActivityRequest(
                userId, username, AuditEventTypes.Create, AuditResourceTypes.User,
                httpContext, new UserActivityDetails
                {
                    ResourceId = userId,
                    Description = "User registration completed"
                });
            await _activityAuditService.LogActivityAsync(activityRequest);
        }
    }
}

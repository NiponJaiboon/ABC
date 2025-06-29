using Core.Entities;
using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// Service for logging authentication events and managing authentication audit trail
/// </summary>
public interface IAuthenticationAuditService
{
    /// <summary>
    /// Log an authentication event
    /// </summary>
    Task LogAuthenticationEventAsync(AuthenticationAuditRequest request);

    /// <summary>
    /// Get authentication logs for a specific user
    /// </summary>
    Task<IEnumerable<AuthenticationAuditLog>> GetUserAuthenticationLogsAsync(
        string userId, AuditLogQuery query);

    /// <summary>
    /// Get recent authentication events for monitoring
    /// </summary>
    Task<IEnumerable<AuthenticationAuditLog>> GetRecentAuthenticationEventsAsync(
        AuditLogQuery query);
}

/// <summary>
/// Service for tracking and managing failed login attempts
/// </summary>
public interface IFailedLoginTrackingService
{
    /// <summary>
    /// Record a failed login attempt
    /// </summary>
    Task RecordFailedAttemptAsync(FailedLoginAttemptRequest request);

    /// <summary>
    /// Get failed login attempts for a user within a time period
    /// </summary>
    Task<IEnumerable<FailedLoginAttempt>> GetFailedAttemptsForUserAsync(
        string userId, TimeSpan timeSpan);

    /// <summary>
    /// Get failed login attempts from an IP address within a time period
    /// </summary>
    Task<IEnumerable<FailedLoginAttempt>> GetFailedAttemptsFromIpAsync(
        string ipAddress, TimeSpan timeSpan);

    /// <summary>
    /// Check if an IP address should be rate limited based on failed attempts
    /// </summary>
    Task<bool> ShouldRateLimitIpAsync(string ipAddress, int maxAttempts = 10,
        TimeSpan? timeWindow = null);

    /// <summary>
    /// Check if a user should be temporarily locked based on failed attempts
    /// </summary>
    Task<bool> ShouldLockUserAsync(string userId, int maxAttempts = 5,
        TimeSpan? timeWindow = null);

    /// <summary>
    /// Clean up old failed login attempts
    /// </summary>
    Task CleanupOldAttemptsAsync(TimeSpan retentionPeriod);

    /// <summary>
    /// Get suspicious login patterns for security analysis
    /// </summary>
    Task<IEnumerable<FailedLoginAttempt>> GetSuspiciousLoginPatternsAsync(
        AuditLogQuery query);
}

/// <summary>
/// Service for logging user activities for compliance and audit trail
/// </summary>
public interface IUserActivityAuditService
{
    /// <summary>
    /// Log a user activity event
    /// </summary>
    Task LogActivityAsync(UserActivityAuditRequest request);

    /// <summary>
    /// Get activity logs for a specific user
    /// </summary>
    Task<IEnumerable<UserActivityAuditLog>> GetUserActivityLogsAsync(
        string userId, AuditLogQuery query);

    /// <summary>
    /// Get activity logs for a specific resource
    /// </summary>
    Task<IEnumerable<UserActivityAuditLog>> GetResourceActivityLogsAsync(
        AuditLogQuery query);

    /// <summary>
    /// Get recent user activities for monitoring
    /// </summary>
    Task<IEnumerable<UserActivityAuditLog>> GetRecentActivitiesAsync(AuditLogQuery query);

    /// <summary>
    /// Clean up old activity logs
    /// </summary>
    Task CleanupOldLogsAsync(TimeSpan retentionPeriod);
}

/// <summary>
/// Service for logging security events and managing security audit trail
/// </summary>
public interface ISecurityAuditService
{
    /// <summary>
    /// Log a security event
    /// </summary>
    Task LogSecurityEventAsync(SecurityAuditRequest request);

    /// <summary>
    /// Get security logs for analysis
    /// </summary>
    Task<IEnumerable<SecurityAuditLog>> GetSecurityLogsAsync(AuditLogQuery query);

    /// <summary>
    /// Get unresolved security events that need investigation
    /// </summary>
    Task<IEnumerable<SecurityAuditLog>> GetUnresolvedSecurityEventsAsync(
        SecuritySeverity minSeverity = SecuritySeverity.Medium);

    /// <summary>
    /// Mark a security event as investigated
    /// </summary>
    Task MarkAsInvestigatedAsync(Guid securityLogId, string investigationNotes);

    /// <summary>
    /// Get security events for a specific IP address
    /// </summary>
    Task<IEnumerable<SecurityAuditLog>> GetSecurityEventsForIpAsync(
        string ipAddress, TimeSpan timeSpan);

    /// <summary>
    /// Clean up old security logs
    /// </summary>
    Task CleanupOldSecurityLogsAsync(TimeSpan retentionPeriod);
}

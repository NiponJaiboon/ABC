using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Updated AuthenticationAuditService implementing the new IAuthenticationAuditService interface
/// </summary>
public class EnhancedAuthenticationAuditService : IAuthenticationAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnhancedAuthenticationAuditService> _logger;

    public EnhancedAuthenticationAuditService(
        ApplicationDbContext context,
        ILogger<EnhancedAuthenticationAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAuthenticationEventAsync(AuthenticationAuditRequest request)
    {
        try
        {
            var auditLog = new AuthenticationAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Username = request.Username,
                Email = request.Email,
                EventType = request.EventType,
                Result = request.Result,
                FailureReason = request.FailureReason,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                AuthenticationMethod = request.AuthenticationMethod,
                SessionId = request.SessionId,
                AdditionalData = request.AdditionalData,
                Timestamp = DateTime.UtcNow
            };

            _context.AuthenticationAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Authentication event logged: {EventType} for user {Username} from {IpAddress} - Result: {Result}",
                request.EventType, request.Username ?? "Unknown", request.IpAddress, request.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log authentication event: {EventType} for user {Username}",
                request.EventType, request.Username ?? "Unknown");
            // Don't rethrow to avoid breaking the authentication flow
        }
    }

    public async Task<IEnumerable<AuthenticationAuditLog>> GetUserAuthenticationLogsAsync(
        string userId, AuditLogQuery query)
    {
        var queryable = _context.AuthenticationAuditLogs
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (query.FromDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp <= query.ToDate.Value);

        if (!string.IsNullOrEmpty(query.EventType))
            queryable = queryable.Where(x => x.EventType == query.EventType);

        if (query.AuthResult.HasValue)
            queryable = queryable.Where(x => x.Result == query.AuthResult.Value);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuthenticationAuditLog>> GetRecentAuthenticationEventsAsync(
        AuditLogQuery query)
    {
        var queryable = _context.AuthenticationAuditLogs.AsQueryable();

        if (query.FromDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp <= query.ToDate.Value);

        if (!string.IsNullOrEmpty(query.EventType))
            queryable = queryable.Where(x => x.EventType == query.EventType);

        if (query.AuthResult.HasValue)
            queryable = queryable.Where(x => x.Result == query.AuthResult.Value);

        if (!string.IsNullOrEmpty(query.UserId))
            queryable = queryable.Where(x => x.UserId == query.UserId);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }
}

// Keep the old service for backward compatibility during migration
public interface ILegacyAuthenticationAuditService
{
    Task LogAuthenticationAttemptAsync(string userId, string method, string provider, bool success, string? ipAddress = null, string? userAgent = null);
    Task LogExternalLoginAttemptAsync(string provider, string email, bool success, bool isNewUser, string? ipAddress = null);
    Task LogAccountLinkingAsync(string userId, string provider, bool success, string action = "link");
    Task LogPasswordChangeAsync(string userId, bool success, string? ipAddress = null);
    Task LogSecurityEventAsync(string userId, string eventType, string description, string? ipAddress = null);
}

public class LegacyAuthenticationAuditService : ILegacyAuthenticationAuditService
{
    private readonly ILogger<LegacyAuthenticationAuditService> _logger;

    public LegacyAuthenticationAuditService(ILogger<LegacyAuthenticationAuditService> logger)
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

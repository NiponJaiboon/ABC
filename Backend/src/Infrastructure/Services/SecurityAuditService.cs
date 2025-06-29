using System.Text.Json;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Service for logging security events and managing security audit trail
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(
        ApplicationDbContext context,
        ILogger<SecurityAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogSecurityEventAsync(SecurityAuditRequest request)
    {
        try
        {
            var securityLog = new SecurityAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EventType = request.EventType,
                Severity = request.Severity,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                RequestPath = request.RequestPath,
                Description = request.Description,
                AdditionalData = SerializeObject(request.AdditionalData),
                Timestamp = DateTime.UtcNow,
                Investigated = false
            };

            _context.SecurityAuditLogs.Add(securityLog);
            await _context.SaveChangesAsync();

            var logLevel = request.Severity switch
            {
                SecuritySeverity.Critical => LogLevel.Critical,
                SecuritySeverity.High => LogLevel.Error,
                SecuritySeverity.Medium => LogLevel.Warning,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel,
                "Security event logged: {EventType} ({Severity}) from {IpAddress} - {Description}",
                request.EventType, request.Severity, request.IpAddress, request.Description ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log security event: {EventType} from {IpAddress}",
                request.EventType, request.IpAddress);
        }
    }

    public async Task<IEnumerable<SecurityAuditLog>> GetSecurityLogsAsync(AuditLogQuery query)
    {
        var queryable = _context.SecurityAuditLogs.AsQueryable();

        if (query.FromDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp <= query.ToDate.Value);

        if (!string.IsNullOrEmpty(query.EventType))
            queryable = queryable.Where(x => x.EventType == query.EventType);

        if (query.SecuritySeverity.HasValue)
            queryable = queryable.Where(x => x.Severity == query.SecuritySeverity.Value);

        if (!string.IsNullOrEmpty(query.UserId))
            queryable = queryable.Where(x => x.UserId == query.UserId);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }

    public async Task<IEnumerable<SecurityAuditLog>> GetUnresolvedSecurityEventsAsync(
        SecuritySeverity minSeverity = SecuritySeverity.Medium)
    {
        return await _context.SecurityAuditLogs
            .Where(x => !x.Investigated && x.Severity >= minSeverity)
            .OrderByDescending(x => x.Severity)
            .ThenByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task MarkAsInvestigatedAsync(Guid securityLogId, string investigationNotes)
    {
        try
        {
            var securityLog = await _context.SecurityAuditLogs
                .FirstOrDefaultAsync(x => x.Id == securityLogId);

            if (securityLog != null)
            {
                securityLog.Investigated = true;
                securityLog.InvestigationNotes = investigationNotes;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Security event {EventId} marked as investigated: {Notes}",
                    securityLogId, investigationNotes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to mark security event {EventId} as investigated",
                securityLogId);
        }
    }

    public async Task<IEnumerable<SecurityAuditLog>> GetSecurityEventsForIpAsync(
        string ipAddress, TimeSpan timeSpan)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);
        return await _context.SecurityAuditLogs
            .Where(x => x.IpAddress == ipAddress && x.Timestamp >= cutoffTime)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task CleanupOldSecurityLogsAsync(TimeSpan retentionPeriod)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(retentionPeriod);
            var oldLogs = await _context.SecurityAuditLogs
                .Where(x => x.Timestamp < cutoffTime && x.Investigated)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.SecurityAuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} old security logs older than {Days} days",
                    oldLogs.Count, retentionPeriod.TotalDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old security logs");
        }
    }

    private static string? SerializeObject(object? obj)
    {
        if (obj == null)
            return null;

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return obj.ToString();
        }
    }
}

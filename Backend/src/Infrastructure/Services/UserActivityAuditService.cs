using System.Text.Json;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Service for logging user activities for compliance and audit trail
/// </summary>
public class UserActivityAuditService : IUserActivityAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserActivityAuditService> _logger;

    public UserActivityAuditService(
        ApplicationDbContext context,
        ILogger<UserActivityAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActivityAsync(UserActivityAuditRequest request)
    {
        try
        {
            var activityLog = new UserActivityAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Username = request.Username,
                Action = request.Action,
                Resource = request.Resource,
                ResourceId = request.ResourceId,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Details = request.Details,
                OldValues = SerializeObject(request.OldValues),
                NewValues = SerializeObject(request.NewValues),
                Timestamp = DateTime.UtcNow
            };

            _context.UserActivityAuditLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User activity logged: {Username} performed {Action} on {Resource} {ResourceId}",
                request.Username, request.Action, request.Resource, request.ResourceId ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log user activity: {Username} - {Action} on {Resource}",
                request.Username, request.Action, request.Resource);
        }
    }

    public async Task<IEnumerable<UserActivityAuditLog>> GetUserActivityLogsAsync(
        string userId, AuditLogQuery query)
    {
        var queryable = _context.UserActivityAuditLogs
            .Where(x => x.UserId == userId)
            .AsQueryable();

        ApplyFilters(ref queryable, query);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserActivityAuditLog>> GetResourceActivityLogsAsync(
        AuditLogQuery query)
    {
        var queryable = _context.UserActivityAuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(query.Resource))
            queryable = queryable.Where(x => x.Resource == query.Resource);

        if (!string.IsNullOrEmpty(query.ResourceId))
            queryable = queryable.Where(x => x.ResourceId == query.ResourceId);

        ApplyFilters(ref queryable, query);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserActivityAuditLog>> GetRecentActivitiesAsync(AuditLogQuery query)
    {
        var queryable = _context.UserActivityAuditLogs.AsQueryable();

        ApplyFilters(ref queryable, query);

        if (!string.IsNullOrEmpty(query.UserId))
            queryable = queryable.Where(x => x.UserId == query.UserId);

        if (!string.IsNullOrEmpty(query.EventType))
            queryable = queryable.Where(x => x.Action == query.EventType);

        return await queryable
            .OrderByDescending(x => x.Timestamp)
            .Take(query.MaxRecords)
            .ToListAsync();
    }

    public async Task CleanupOldLogsAsync(TimeSpan retentionPeriod)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(retentionPeriod);
            var oldLogs = await _context.UserActivityAuditLogs
                .Where(x => x.Timestamp < cutoffTime)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.UserActivityAuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} old user activity logs older than {Days} days",
                    oldLogs.Count, retentionPeriod.TotalDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old user activity logs");
        }
    }

    private static void ApplyFilters(ref IQueryable<UserActivityAuditLog> queryable, AuditLogQuery query)
    {
        if (query.FromDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(x => x.Timestamp <= query.ToDate.Value);
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

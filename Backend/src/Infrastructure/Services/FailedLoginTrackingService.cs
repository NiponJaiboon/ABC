using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Service for tracking and managing failed login attempts
/// </summary>
public class FailedLoginTrackingService : IFailedLoginTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FailedLoginTrackingService> _logger;

    public FailedLoginTrackingService(
        ApplicationDbContext context,
        ILogger<FailedLoginTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RecordFailedAttemptAsync(FailedLoginAttemptRequest request)
    {
        try
        {
            var failedAttempt = new FailedLoginAttempt
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Username = request.Username,
                Email = request.Email,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                FailureReason = request.FailureReason,
                AdditionalData = request.AdditionalData,
                AttemptTime = DateTime.UtcNow
            };

            _context.FailedLoginAttempts.Add(failedAttempt);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Failed login attempt recorded: {Username} from {IpAddress} - Reason: {FailureReason}",
                request.Username ?? "Unknown", request.IpAddress, request.FailureReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to record failed login attempt for user {Username} from {IpAddress}",
                request.Username ?? "Unknown", request.IpAddress);
        }
    }

    public async Task<IEnumerable<FailedLoginAttempt>> GetFailedAttemptsForUserAsync(
        string userId, TimeSpan timeSpan)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);
        return await _context.FailedLoginAttempts
            .Where(x => x.UserId == userId && x.AttemptTime >= cutoffTime)
            .OrderByDescending(x => x.AttemptTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<FailedLoginAttempt>> GetFailedAttemptsFromIpAsync(
        string ipAddress, TimeSpan timeSpan)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);
        return await _context.FailedLoginAttempts
            .Where(x => x.IpAddress == ipAddress && x.AttemptTime >= cutoffTime)
            .OrderByDescending(x => x.AttemptTime)
            .ToListAsync();
    }

    public async Task<bool> ShouldRateLimitIpAsync(string ipAddress, int maxAttempts = 10,
        TimeSpan? timeWindow = null)
    {
        var window = timeWindow ?? TimeSpan.FromMinutes(15);
        var cutoffTime = DateTime.UtcNow.Subtract(window);

        var attemptCount = await _context.FailedLoginAttempts
            .CountAsync(x => x.IpAddress == ipAddress && x.AttemptTime >= cutoffTime);

        return attemptCount >= maxAttempts;
    }

    public async Task<bool> ShouldLockUserAsync(string userId, int maxAttempts = 5,
        TimeSpan? timeWindow = null)
    {
        var window = timeWindow ?? TimeSpan.FromMinutes(15);
        var cutoffTime = DateTime.UtcNow.Subtract(window);

        var attemptCount = await _context.FailedLoginAttempts
            .CountAsync(x => x.UserId == userId && x.AttemptTime >= cutoffTime);

        return attemptCount >= maxAttempts;
    }

    public async Task CleanupOldAttemptsAsync(TimeSpan retentionPeriod)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(retentionPeriod);
            var oldAttempts = await _context.FailedLoginAttempts
                .Where(x => x.AttemptTime < cutoffTime)
                .ToListAsync();

            if (oldAttempts.Any())
            {
                _context.FailedLoginAttempts.RemoveRange(oldAttempts);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} old failed login attempts older than {Days} days",
                    oldAttempts.Count, retentionPeriod.TotalDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old failed login attempts");
        }
    }

    public async Task<IEnumerable<FailedLoginAttempt>> GetSuspiciousLoginPatternsAsync(
        AuditLogQuery query)
    {
        var queryable = _context.FailedLoginAttempts.AsQueryable();

        if (query.FromDate.HasValue)
            queryable = queryable.Where(x => x.AttemptTime >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(x => x.AttemptTime <= query.ToDate.Value);

        // Group by IP and find IPs with multiple failed attempts from different users
        var suspiciousAttempts = await queryable
            .GroupBy(x => x.IpAddress)
            .Where(g => g.Select(x => x.UserId).Distinct().Count() > 3) // Multiple users from same IP
            .SelectMany(g => g)
            .OrderByDescending(x => x.AttemptTime)
            .Take(query.MaxRecords)
            .ToListAsync();

        return suspiciousAttempts;
    }
}

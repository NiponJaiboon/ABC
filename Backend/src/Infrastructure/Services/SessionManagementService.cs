using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SessionManagementService : ISessionManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SessionManagementService> _logger;

    public SessionManagementService(
        ApplicationDbContext context,
        ILogger<SessionManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SessionInfo> CreateSessionAsync(string userId, string ipAddress,
        string userAgent, string deviceName, AuthType authType)
    {
        try
        {
            // Check if user has too many active sessions
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                .CountAsync();

            if (activeSessions >= HybridAuthConstants.SessionPolicy.MaxSessionsPerUser)
            {
                // Remove oldest session
                var oldestSession = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .OrderBy(s => s.LastAccessed)
                    .FirstOrDefaultAsync();

                if (oldestSession != null)
                {
                    oldestSession.IsActive = false;
                    _logger.LogInformation("Revoked oldest session {SessionId} for user {UserId} due to session limit",
                        oldestSession.SessionId, userId);
                }
            }

            var session = new UserSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(HybridAuthConstants.SessionPolicy.SessionTimeoutMinutes),
                LastAccessed = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceName = deviceName,
                AuthType = authType,
                IsActive = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created session {SessionId} for user {UserId}", session.SessionId, userId);

            return MapToSessionInfo(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SessionInfo?> GetSessionAsync(string sessionId)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            return session != null ? MapToSessionInfo(session) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<List<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        try
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastAccessed)
                .ToListAsync();

            return sessions.Select(MapToSessionInfo).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null || !session.IsActive || session.ExpiresAt <= DateTime.UtcNow)
            {
                if (session != null && session.IsActive && session.ExpiresAt <= DateTime.UtcNow)
                {
                    // Mark expired session as inactive
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Marked expired session {SessionId} as inactive", sessionId);
                }
                return false;
            }

            // Update last accessed time (sliding expiration)
            await UpdateSessionLastAccessedAsync(sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task UpdateSessionLastAccessedAsync(string sessionId)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null && session.IsActive)
            {
                var now = DateTime.UtcNow;
                session.LastAccessed = now;

                // Implement sliding expiration
                var timeUntilExpiry = session.ExpiresAt - now;
                var slidingWindow = TimeSpan.FromMinutes(HybridAuthConstants.SessionPolicy.SlidingExpirationMinutes);

                if (timeUntilExpiry < slidingWindow)
                {
                    session.ExpiresAt = now.AddMinutes(HybridAuthConstants.SessionPolicy.SessionTimeoutMinutes);
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session last accessed time {SessionId}", sessionId);
        }
    }

    public async Task<bool> RevokeSessionAsync(string sessionId)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null && session.IsActive)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Revoked session {SessionId} for user {UserId}", sessionId, session.UserId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<int> RevokeUserSessionsAsync(string userId, string? exceptSessionId = null)
    {
        try
        {
            var sessionsToRevoke = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive &&
                           (exceptSessionId == null || s.SessionId != exceptSessionId))
                .ToListAsync();

            foreach (var session in sessionsToRevoke)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked {Count} sessions for user {UserId}", sessionsToRevoke.Count, userId);
            return sessionsToRevoke.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> CleanupExpiredSessionsAsync()
    {
        try
        {
            var expiredSessions = await _context.UserSessions
                .Where(s => s.IsActive && s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            return expiredSessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            throw;
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(string userId, string? currentSessionId = null)
    {
        try
        {
            var allSessions = await GetUserSessionsAsync(userId);
            var currentSession = currentSessionId != null
                ? allSessions.FirstOrDefault(s => s.SessionId == currentSessionId)
                : null;

            return new SessionStatusResponse
            {
                IsActive = currentSession?.IsActive ?? false,
                Session = currentSession,
                AllSessions = allSessions,
                ActiveSessionCount = allSessions.Count(s => s.IsActive)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ExtendSessionAsync(string sessionId, TimeSpan? extension = null)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null && session.IsActive)
            {
                var extensionTime = extension ?? TimeSpan.FromMinutes(HybridAuthConstants.SessionPolicy.SessionTimeoutMinutes);
                session.ExpiresAt = DateTime.UtcNow.Add(extensionTime);
                session.LastAccessed = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Extended session {SessionId} by {Extension}", sessionId, extensionTime);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session {SessionId}", sessionId);
            throw;
        }
    }

    private static SessionInfo MapToSessionInfo(UserSession session)
    {
        return new SessionInfo
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt,
            LastAccessed = session.LastAccessed,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            DeviceName = session.DeviceName,
            AuthType = session.AuthType,
            IsActive = session.IsActive
        };
    }
}

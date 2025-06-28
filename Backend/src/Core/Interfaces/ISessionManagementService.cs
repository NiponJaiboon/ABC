using Core.Models;

namespace Core.Interfaces;

public interface ISessionManagementService
{
    Task<SessionInfo> CreateSessionAsync(string userId, string ipAddress, string userAgent,
        string deviceName, AuthType authType);
    Task<SessionInfo?> GetSessionAsync(string sessionId);
    Task<List<SessionInfo>> GetUserSessionsAsync(string userId);
    Task<bool> ValidateSessionAsync(string sessionId);
    Task UpdateSessionLastAccessedAsync(string sessionId);
    Task<bool> RevokeSessionAsync(string sessionId);
    Task<int> RevokeUserSessionsAsync(string userId, string? exceptSessionId = null);
    Task<int> CleanupExpiredSessionsAsync();
    Task<SessionStatusResponse> GetSessionStatusAsync(string userId, string? currentSessionId = null);
    Task<bool> ExtendSessionAsync(string sessionId, TimeSpan? extension = null);
}

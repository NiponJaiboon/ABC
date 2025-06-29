using Core.Entities;

namespace Core.Models
{
    /// <summary>
    /// DTO for authentication audit logging
    /// </summary>
    public class AuthenticationAuditRequest
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string EventType { get; set; } = string.Empty;
        public AuthenticationResult Result { get; set; }
        public string? FailureReason { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? AuthenticationMethod { get; set; }
        public string? SessionId { get; set; }
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// DTO for failed login attempt tracking
    /// </summary>
    public class FailedLoginAttemptRequest
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// DTO for user activity audit logging
    /// </summary>
    public class UserActivityAuditRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? ResourceId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Details { get; set; }
        public object? OldValues { get; set; }
        public object? NewValues { get; set; }
    }

    /// <summary>
    /// DTO for security audit logging
    /// </summary>
    public class SecurityAuditRequest
    {
        public string? UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public SecuritySeverity Severity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? RequestPath { get; set; }
        public string? Description { get; set; }
        public object? AdditionalData { get; set; }
    }

    /// <summary>
    /// Query parameters for audit log filtering
    /// </summary>
    public class AuditLogQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int MaxRecords { get; set; } = 100;
        public string? UserId { get; set; }
        public string? EventType { get; set; }
        public string? Resource { get; set; }
        public string? ResourceId { get; set; }
        public AuthenticationResult? AuthResult { get; set; }
        public SecuritySeverity? SecuritySeverity { get; set; }
    }

    /// <summary>
    /// Constants for audit event types
    /// </summary>
    public static class AuditEventTypes
    {
        // Authentication Events
        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";
        public const string Registration = "REGISTRATION";
        public const string PasswordChange = "PASSWORD_CHANGE";
        public const string PasswordReset = "PASSWORD_RESET";
        public const string ExternalAuth = "EXTERNAL_AUTH";
        public const string TwoFactorAuth = "TWO_FACTOR_AUTH";
        public const string AccountLockout = "ACCOUNT_LOCKOUT";
        public const string AccountUnlock = "ACCOUNT_UNLOCK";

        // User Activity Events
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string View = "VIEW";
        public const string Download = "DOWNLOAD";
        public const string Upload = "UPLOAD";

        // Security Events
        public const string SuspiciousActivity = "SUSPICIOUS_ACTIVITY";
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
        public const string UnauthorizedAccess = "UNAUTHORIZED_ACCESS";
        public const string SecurityPolicyViolation = "SECURITY_POLICY_VIOLATION";
        public const string DataBreach = "DATA_BREACH";
        public const string MaliciousRequest = "MALICIOUS_REQUEST";
    }

    /// <summary>
    /// Constants for audit resource types
    /// </summary>
    public static class AuditResourceTypes
    {
        public const string User = "USER";
        public const string Portfolio = "PORTFOLIO";
        public const string Project = "PROJECT";
        public const string Skill = "SKILL";
        public const string ProjectSkill = "PROJECT_SKILL";
        public const string Session = "SESSION";
        public const string ApiKey = "API_KEY";
        public const string OAuthClient = "OAUTH_CLIENT";
        public const string Permission = "PERMISSION";
        public const string Role = "ROLE";
        public const string Scope = "SCOPE";
    }
}

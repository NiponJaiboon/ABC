using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Audit log for authentication events (login, logout, registration, etc.)
    /// </summary>
    public class AuthenticationAuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // LOGIN, LOGOUT, REGISTER, PASSWORD_CHANGE, etc.

        [Required]
        public AuthenticationResult Result { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(100)]
        public string? AuthenticationMethod { get; set; } // LOCAL, GOOGLE, MICROSOFT, etc.

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? AdditionalData { get; set; } // JSON for extra context

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// Track failed login attempts for security monitoring and account lockout
    /// </summary>
    public class FailedLoginAttempt
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        [MaxLength(200)]
        public string FailureReason { get; set; } = string.Empty;

        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? AdditionalData { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// General user activity audit trail for compliance and investigation
    /// </summary>
    public class UserActivityAuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, VIEW, etc.

        [Required]
        [MaxLength(100)]
        public string Resource { get; set; } = string.Empty; // Portfolio, Project, User, etc.

        [MaxLength(450)]
        public string? ResourceId { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Details { get; set; } // JSON for detailed information

        [MaxLength(1000)]
        public string? OldValues { get; set; } // JSON for before state

        [MaxLength(1000)]
        public string? NewValues { get; set; } // JSON for after state

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }

    /// <summary>
    /// Security events audit log for monitoring suspicious activities
    /// </summary>
    public class SecurityAuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty; // SUSPICIOUS_ACTIVITY, RATE_LIMIT_EXCEEDED, etc.

        [Required]
        public SecuritySeverity Severity { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(200)]
        public string? RequestPath { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? AdditionalData { get; set; } // JSON for detailed information

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool Investigated { get; set; } = false;

        [MaxLength(1000)]
        public string? InvestigationNotes { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }

    /// <summary>
    /// Enumeration for authentication results
    /// </summary>
    public enum AuthenticationResult
    {
        Success = 0,
        Failed = 1,
        LockedOut = 2,
        NotAllowed = 3,
        RequiresTwoFactor = 4,
        InvalidCredentials = 5,
        AccountNotFound = 6,
        ExternalProviderError = 7
    }

    /// <summary>
    /// Enumeration for security event severity levels
    /// </summary>
    public enum SecuritySeverity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}

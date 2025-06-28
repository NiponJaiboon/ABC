using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace Core.Entities;

public class UserSession
{
    [Key]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

    public string IpAddress { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string DeviceName { get; set; } = string.Empty;

    public AuthType AuthType { get; set; } = AuthType.Hybrid;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation property
    public virtual ApplicationUser User { get; set; }
}

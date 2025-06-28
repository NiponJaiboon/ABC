#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

// Step 12: Authorization & Scopes Entities

public class OAuthClient
{
    [Key]
    public string ClientId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ClientType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    [Required]
    public string RedirectUris { get; set; } = string.Empty; // JSON array

    public string PostLogoutRedirectUris { get; set; } = string.Empty; // JSON array

    [Required]
    public string Scopes { get; set; } = string.Empty; // JSON array

    public string GrantTypes { get; set; } = string.Empty; // JSON array

    public bool RequirePkce { get; set; } = true;

    public bool RequireClientSecret { get; set; } = true;

    [MaxLength(500)]
    public string? ClientUri { get; set; }

    [MaxLength(500)]
    public string? LogoUri { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? CreatedBy { get; set; } // Nullable for system clients

    public DateTime? LastUsed { get; set; }

    public int UsageCount { get; set; } = 0;

    // Navigation properties
    public virtual ApplicationUser? Creator { get; set; }
    public virtual ICollection<UserConsent> UserConsents { get; set; } = new List<UserConsent>();
}

public class UserConsent
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string GrantedScopes { get; set; } = string.Empty; // JSON array

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime ConsentedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public bool RememberConsent { get; set; } = true;

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual OAuthClient? Client { get; set; }
}

public class UserPermission
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Permission { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Source { get; set; } = "manual"; // manual, role, client

    [MaxLength(100)]
    public string? SourceId { get; set; } // role name or client ID

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required]
    public string GrantedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? GrantedByUser { get; set; }
}

public class ScopeDefinitionEntity
{
    [Key]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = false;

    public bool IsDefault { get; set; } = false;

    public string Permissions { get; set; } = string.Empty; // JSON array

    [MaxLength(50)]
    public string Category { get; set; } = "custom";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

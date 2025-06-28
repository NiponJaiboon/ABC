using System.ComponentModel.DataAnnotations;
using Core.Constants;

namespace Core.Models;

// Step 12: Authorization & Scopes Models

public class ClientRegistrationModel
{
    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string ClientType { get; set; } = ClientTypes.Web;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public List<string> RedirectUris { get; set; } = new();

    public List<string> PostLogoutRedirectUris { get; set; } = new();

    [Required]
    public List<string> Scopes { get; set; } = new();

    public List<string> GrantTypes { get; set; } = new();

    public bool RequirePkce { get; set; } = true;

    public bool RequireClientSecret { get; set; } = true;

    public string? ClientUri { get; set; }

    public string? LogoUri { get; set; }

    public string? ContactEmail { get; set; }
}

public class ClientRegistrationResult
{
    public bool Success { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Errors { get; set; } = new();
    public ClientInfo Client { get; set; } = new();
}

public class ClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    public List<string> Scopes { get; set; } = new();
    public List<string> GrantTypes { get; set; } = new();
    public bool RequirePkce { get; set; }
    public bool RequireClientSecret { get; set; }
    public string? ClientUri { get; set; }
    public string? LogoUri { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class ScopeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsDefault { get; set; }
    public List<string> Permissions { get; set; } = new();
    public string Category { get; set; } = string.Empty;
}

public class AuthorizeRequest
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ResponseType { get; set; } = "code";

    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    public string Scope { get; set; } = "openid profile email";

    public string? State { get; set; }

    public string? CodeChallenge { get; set; }

    public string? CodeChallengeMethod { get; set; } = "S256";

    public string? Nonce { get; set; }

    public string? Prompt { get; set; }

    public int? MaxAge { get; set; }
}

public class AuthorizeResult
{
    public string ClientId { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public string? ResponseType { get; set; }
    public string? Scope { get; set; }
    public string? State { get; set; }
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
    public List<string> RequestedScopes { get; set; } = new();
}

public class ConsentModel
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientDescription { get; set; } = string.Empty;
    public string? ClientUri { get; set; }
    public string? LogoUri { get; set; }
    public List<ScopeConsentItem> RequestedScopes { get; set; } = new();
    public List<ScopeConsentDetail> ScopeDetails { get; set; } = new();
    public bool HasExistingConsent { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public bool RememberConsent { get; set; } = true;
}

public class ScopeConsentDetail
{
    public string Name { get; set; } = string.Empty;
    public string ScopeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsGranted { get; set; }
    public bool IsAlreadyConsented { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class ScopeConsentItem
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsGranted { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class ConsentResponse
{
    public bool Granted { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
    public List<string> GrantedScopes { get; set; } = new();
    public bool RememberConsent { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

public class UserPermissionModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public List<string> Scopes { get; set; } = new();
    public int DirectPermissionCount { get; set; }
    public int RolePermissionCount { get; set; }
    public int TotalPermissionCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PermissionAssignmentModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public List<string> Permissions { get; set; } = new();

    public List<string> Scopes { get; set; } = new();

    public DateTime? ExpiresAt { get; set; }

    public string? Reason { get; set; }
}

public class PermissionDetail
{
    public string Permission { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? SourceId { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsRevoked { get; set; }
    public string? Reason { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
}

public class ScopeValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidScopes { get; set; } = new();
    public List<string> InvalidScopes { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ClientListResponse
{
    public List<ClientInfo> Clients { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class ClientUpdateModel
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ClientName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public List<string>? RedirectUris { get; set; }

    public List<string>? PostLogoutRedirectUris { get; set; }

    public List<string>? Scopes { get; set; }

    public bool? RequirePkce { get; set; }

    public string? ClientUri { get; set; }

    public string? LogoUri { get; set; }

    public bool? IsActive { get; set; }
}

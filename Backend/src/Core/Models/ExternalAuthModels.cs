using System.ComponentModel.DataAnnotations;
using Core.Entities;

namespace Core.Models;

public class ExternalLoginModel
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class ExternalLoginCallbackModel
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;
}

public class LinkExternalLoginModel
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderKey { get; set; } = string.Empty;

    [Required]
    public string ProviderDisplayName { get; set; } = string.Empty;
}

public class RemoveExternalLoginModel
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderKey { get; set; } = string.Empty;
}

public class ExternalUserInfo
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class ExternalProviderInfo
{
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public DateTimeOffset DateLinked { get; set; }
    public bool IsPrimary { get; set; }
}

public class ExternalAuthResult
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public ApplicationUser User { get; set; }
    public bool IsNewUser { get; set; }
    public ExternalUserInfo ExternalUserInfo { get; set; }
    public List<string> Errors { get; set; } = new();
    public string RedirectUrl { get; set; } = string.Empty;
}

public class ValidatePasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public int StrengthScore { get; set; } // 0-100
    public string StrengthLevel { get; set; } = "Weak"; // Weak, Fair, Good, Strong, Very Strong
}

// Enhanced Account Linking Models for Step 9
public class LinkAccountRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    public bool ForceLink { get; set; } = false;

    public string UserConsent { get; set; } = string.Empty; // For conflict resolution
}

public class LinkAccountResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public bool RequiresConfirmation { get; set; } = false;
    public AccountLinkingConflict Conflict { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
}

public class AccountLinkingConflict
{
    public string ConflictType { get; set; } = string.Empty; // EmailMismatch, AccountExists, DuplicateProvider
    public string ConflictDescription { get; set; } = string.Empty;
    public string ExistingUserEmail { get; set; } = string.Empty;
    public string IncomingUserEmail { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public List<string> ResolutionOptions { get; set; } = new();
    public string ConflictToken { get; set; } = string.Empty; // For resolving conflicts
}

public class ConflictResolutionRequest
{
    [Required]
    public string ConflictToken { get; set; } = string.Empty;

    [Required]
    public string Resolution { get; set; } = string.Empty; // Link, Replace, Cancel

    public string Password { get; set; } = string.Empty; // For identity verification

    public bool ConfirmAction { get; set; } = false;
}

public class AccountLinkingSummary
{
    public int TotalLinkedAccounts { get; set; }
    public List<LinkedAccountInfo> LinkedAccounts { get; set; } = new();
    public List<string> AvailableProviders { get; set; } = new();
    public bool HasPassword { get; set; }
    public bool CanUnlinkAccounts { get; set; }
    public List<string> SecurityRecommendations { get; set; } = new();
    public AccountSecurityScore SecurityScore { get; set; } = new();
}

public class LinkedAccountInfo
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateLinked { get; set; }
    public bool IsPrimary { get; set; }
    public bool CanUnlink { get; set; } = true;
    public string IconUrl { get; set; } = string.Empty;
    public DateTime? LastUsed { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Revoked
}

public class AccountSecurityScore
{
    public int OverallScore { get; set; } // 0-100
    public string SecurityLevel { get; set; } = "Basic"; // Basic, Good, Strong, Excellent
    public List<string> PositiveFactors { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public bool HasTwoFactorMethods { get; set; }
    public int AuthenticationMethodsCount { get; set; }
}

public class BulkAccountAction
{
    public string Action { get; set; } = string.Empty; // UnlinkAll, SetPrimary, UpdateStatus
    public List<string> ProviderKeys { get; set; } = new();
    public string NewPrimaryProvider { get; set; } = string.Empty;
    public bool RequirePasswordConfirmation { get; set; } = true;
    public string Password { get; set; } = string.Empty;
}

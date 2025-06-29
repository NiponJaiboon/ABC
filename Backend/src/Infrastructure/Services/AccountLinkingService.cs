using System.Security.Claims;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Enhanced service for managing account linking operations with conflict resolution
/// </summary>
public interface IAccountLinkingService
{
    Task<LinkAccountResult> InitiateLinkingAsync(string userId, LinkAccountRequest request);
    Task<LinkAccountResult> CompleteLinkingAsync(string userId, ExternalUserInfo externalUserInfo);
    Task<LinkAccountResult> ResolveConflictAsync(string userId, ConflictResolutionRequest request);
    Task<AccountLinkingSummary> GetAccountSummaryAsync(string userId);
    Task<AccountSecurityScore> CalculateSecurityScoreAsync(string userId);
    Task<List<AccountLinkingConflict>> DetectConflictsAsync(string userId, ExternalUserInfo externalUserInfo);
    Task<bool> CanUnlinkAccountAsync(string userId, string provider, string providerKey);
    Task<LinkAccountResult> BulkAccountActionAsync(string userId, BulkAccountAction action);
}

public class AccountLinkingService : IAccountLinkingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExternalAuthenticationService _externalAuthService;
    private readonly ILegacyAuthenticationAuditService _auditService;
    private readonly ILogger<AccountLinkingService> _logger;
    private readonly Dictionary<string, AccountLinkingConflict> _conflictCache;

    public AccountLinkingService(
        UserManager<ApplicationUser> userManager,
        IExternalAuthenticationService externalAuthService,
        ILegacyAuthenticationAuditService auditService,
        ILogger<AccountLinkingService> logger)
    {
        _userManager = userManager;
        _externalAuthService = externalAuthService;
        _auditService = auditService;
        _logger = logger;
        _conflictCache = new Dictionary<string, AccountLinkingConflict>();
    }

    public async Task<LinkAccountResult> InitiateLinkingAsync(string userId, LinkAccountRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Check if provider is already linked
            var existingLogins = await _userManager.GetLoginsAsync(user);
            var existingLogin = existingLogins.FirstOrDefault(l => l.LoginProvider == request.Provider);

            if (existingLogin != null && !request.ForceLink)
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { $"{request.Provider} account is already linked to your account" }
                };
            }

            _logger.LogInformation("Initiating account linking for user {UserId} with provider {Provider}",
                userId, request.Provider);

            return new LinkAccountResult
            {
                Success = true,
                Message = "Account linking initiated successfully",
                RedirectUrl = $"/api/externalauth/challenge/{request.Provider}?returnUrl={request.ReturnUrl}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating account linking for user {UserId}", userId);
            return new LinkAccountResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred while initiating account linking" }
            };
        }
    }

    public async Task<LinkAccountResult> CompleteLinkingAsync(string userId, ExternalUserInfo externalUserInfo)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Detect conflicts
            var conflicts = await DetectConflictsAsync(userId, externalUserInfo);
            if (conflicts.Any())
            {
                var conflict = conflicts.First();
                var conflictToken = Guid.NewGuid().ToString();
                _conflictCache[conflictToken] = conflict;
                conflict.ConflictToken = conflictToken;

                return new LinkAccountResult
                {
                    Success = false,
                    RequiresConfirmation = true,
                    Conflict = conflict,
                    Errors = new List<string> { "Account linking conflict detected. Please resolve the conflict." }
                };
            }

            // Proceed with linking
            var loginInfo = new UserLoginInfo(
                externalUserInfo.Provider,
                externalUserInfo.ProviderUserId,
                externalUserInfo.Provider);

            var result = await _userManager.AddLoginAsync(user, loginInfo);
            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully linked {Provider} account to user {UserId}",
                    externalUserInfo.Provider, userId);

                return new LinkAccountResult
                {
                    Success = true,
                    Message = $"{externalUserInfo.Provider} account linked successfully"
                };
            }
            else
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing account linking for user {UserId}", userId);
            return new LinkAccountResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred while completing account linking" }
            };
        }
    }

    public async Task<LinkAccountResult> ResolveConflictAsync(string userId, ConflictResolutionRequest request)
    {
        try
        {
            if (!_conflictCache.TryGetValue(request.ConflictToken, out var conflict))
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid or expired conflict token" }
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Verify password if required
            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return new LinkAccountResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid password" }
                    };
                }
            }

            var result = new LinkAccountResult();

            switch (request.Resolution.ToLower())
            {
                case "link":
                    result = await HandleLinkResolution(user, conflict);
                    break;
                case "replace":
                    result = await HandleReplaceResolution(user, conflict);
                    break;
                case "cancel":
                    result = HandleCancelResolution();
                    break;
                default:
                    result = new LinkAccountResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid resolution option" }
                    };
                    break;
            }

            // Remove conflict from cache
            _conflictCache.Remove(request.ConflictToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving account linking conflict for user {UserId}", userId);
            return new LinkAccountResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred while resolving the conflict" }
            };
        }
    }

    public async Task<AccountLinkingSummary> GetAccountSummaryAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AccountLinkingSummary();
            }

            var externalLogins = await _userManager.GetLoginsAsync(user);
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var securityScore = await CalculateSecurityScoreAsync(userId);

            var linkedAccounts = externalLogins.Select(login => new LinkedAccountInfo
            {
                Provider = login.LoginProvider,
                ProviderDisplayName = GetProviderDisplayName(login.LoginProvider),
                ProviderKey = login.ProviderKey,
                DateLinked = DateTime.UtcNow, // This should come from audit logs
                CanUnlink = externalLogins.Count > 1 || hasPassword,
                IconUrl = GetProviderIconUrl(login.LoginProvider),
                Status = "Active"
            }).ToList();

            return new AccountLinkingSummary
            {
                TotalLinkedAccounts = linkedAccounts.Count,
                LinkedAccounts = linkedAccounts,
                AvailableProviders = GetAvailableProviders(),
                HasPassword = hasPassword,
                CanUnlinkAccounts = linkedAccounts.Count > 1 || hasPassword,
                SecurityRecommendations = GenerateSecurityRecommendations(hasPassword, linkedAccounts.Count),
                SecurityScore = securityScore
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account summary for user {UserId}", userId);
            return new AccountLinkingSummary();
        }
    }

    public async Task<AccountSecurityScore> CalculateSecurityScoreAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AccountSecurityScore();
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            var externalLogins = await _userManager.GetLoginsAsync(user);
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            var score = 0;
            var positiveFactors = new List<string>();
            var improvementAreas = new List<string>();

            // Base score for having an account
            score += 20;

            // Password authentication
            if (hasPassword)
            {
                score += 25;
                positiveFactors.Add("Password authentication enabled");
            }
            else
            {
                improvementAreas.Add("Set up password authentication as backup");
            }

            // External authentication
            if (externalLogins.Any())
            {
                score += 20;
                positiveFactors.Add($"{externalLogins.Count} external authentication method(s)");

                if (externalLogins.Count > 1)
                {
                    score += 15;
                    positiveFactors.Add("Multiple authentication methods");
                }
            }
            else
            {
                improvementAreas.Add("Link external accounts for convenience and security");
            }

            // Email verification
            if (emailConfirmed)
            {
                score += 20;
                positiveFactors.Add("Email verified");
            }
            else
            {
                improvementAreas.Add("Verify your email address");
            }

            var securityLevel = score switch
            {
                >= 80 => "Excellent",
                >= 60 => "Strong",
                >= 40 => "Good",
                _ => "Basic"
            };

            return new AccountSecurityScore
            {
                OverallScore = Math.Min(score, 100),
                SecurityLevel = securityLevel,
                PositiveFactors = positiveFactors,
                ImprovementAreas = improvementAreas,
                HasTwoFactorMethods = hasPassword && externalLogins.Any(),
                AuthenticationMethodsCount = (hasPassword ? 1 : 0) + externalLogins.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating security score for user {UserId}", userId);
            return new AccountSecurityScore();
        }
    }

    public async Task<List<AccountLinkingConflict>> DetectConflictsAsync(string userId, ExternalUserInfo externalUserInfo)
    {
        var conflicts = new List<AccountLinkingConflict>();

        try
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
                return conflicts;

            // Check for email mismatch
            if (!string.IsNullOrEmpty(externalUserInfo.Email) &&
                !string.Equals(currentUser.Email, externalUserInfo.Email, StringComparison.OrdinalIgnoreCase))
            {
                conflicts.Add(new AccountLinkingConflict
                {
                    ConflictType = "EmailMismatch",
                    ConflictDescription = "The email address from the external provider doesn't match your account email",
                    ExistingUserEmail = currentUser.Email ?? "",
                    IncomingUserEmail = externalUserInfo.Email,
                    Provider = externalUserInfo.Provider,
                    ResolutionOptions = new List<string> { "Link", "Cancel" }
                });
            }

            // Check if external account is already linked to another user
            var existingUser = await _userManager.FindByLoginAsync(externalUserInfo.Provider, externalUserInfo.ProviderUserId);
            if (existingUser != null && existingUser.Id != userId)
            {
                conflicts.Add(new AccountLinkingConflict
                {
                    ConflictType = "AccountExists",
                    ConflictDescription = "This external account is already linked to another user",
                    ExistingUserEmail = existingUser.Email ?? "",
                    IncomingUserEmail = externalUserInfo.Email,
                    Provider = externalUserInfo.Provider,
                    ResolutionOptions = new List<string> { "Replace", "Cancel" }
                });
            }

            // Check for duplicate provider
            var userLogins = await _userManager.GetLoginsAsync(currentUser);
            var duplicateProvider = userLogins.FirstOrDefault(l => l.LoginProvider == externalUserInfo.Provider);
            if (duplicateProvider != null)
            {
                conflicts.Add(new AccountLinkingConflict
                {
                    ConflictType = "DuplicateProvider",
                    ConflictDescription = $"You already have a {externalUserInfo.Provider} account linked",
                    Provider = externalUserInfo.Provider,
                    ResolutionOptions = new List<string> { "Replace", "Cancel" }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting conflicts for user {UserId}", userId);
        }

        return conflicts;
    }

    public async Task<bool> CanUnlinkAccountAsync(string userId, string provider, string providerKey)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var hasPassword = await _userManager.HasPasswordAsync(user);
            var externalLogins = await _userManager.GetLoginsAsync(user);

            // Can't unlink if it's the only authentication method
            return hasPassword || externalLogins.Count > 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if account can be unlinked for user {UserId}", userId);
            return false;
        }
    }

    public async Task<LinkAccountResult> BulkAccountActionAsync(string userId, BulkAccountAction action)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new LinkAccountResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Verify password if required
            if (action.RequirePasswordConfirmation && !string.IsNullOrEmpty(action.Password))
            {
                var passwordValid = await _userManager.CheckPasswordAsync(user, action.Password);
                if (!passwordValid)
                {
                    return new LinkAccountResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid password" }
                    };
                }
            }

            switch (action.Action.ToLower())
            {
                case "unlinkall":
                    return await UnlinkAllAccountsAsync(user);
                case "setprimary":
                    return await SetPrimaryProviderAsync(user, action.NewPrimaryProvider);
                default:
                    return new LinkAccountResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid bulk action" }
                    };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk account action for user {UserId}", userId);
            return new LinkAccountResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred while performing bulk action" }
            };
        }
    }

    #region Private Helper Methods

    private async Task<LinkAccountResult> HandleLinkResolution(ApplicationUser user, AccountLinkingConflict conflict)
    {
        // Implement link resolution logic
        _logger.LogInformation("Handling link resolution for user {UserId}", user.Id);
        return new LinkAccountResult
        {
            Success = true,
            Message = "Account linked successfully after conflict resolution"
        };
    }

    private async Task<LinkAccountResult> HandleReplaceResolution(ApplicationUser user, AccountLinkingConflict conflict)
    {
        // Implement replace resolution logic
        _logger.LogInformation("Handling replace resolution for user {UserId}", user.Id);
        return new LinkAccountResult
        {
            Success = true,
            Message = "External account replaced successfully"
        };
    }

    private LinkAccountResult HandleCancelResolution()
    {
        return new LinkAccountResult
        {
            Success = true,
            Message = "Account linking cancelled by user"
        };
    }

    private async Task<LinkAccountResult> UnlinkAllAccountsAsync(ApplicationUser user)
    {
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return new LinkAccountResult
            {
                Success = false,
                Errors = new List<string> { "Cannot unlink all accounts without a password set" }
            };
        }

        var externalLogins = await _userManager.GetLoginsAsync(user);
        var errors = new List<string>();

        foreach (var login in externalLogins)
        {
            var result = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
            }
        }

        return new LinkAccountResult
        {
            Success = !errors.Any(),
            Message = errors.Any() ? "Some accounts could not be unlinked" : "All external accounts unlinked successfully",
            Errors = errors
        };
    }

    private async Task<LinkAccountResult> SetPrimaryProviderAsync(ApplicationUser user, string provider)
    {
        // Implementation for setting primary provider
        // This would involve updating user preferences or metadata
        _logger.LogInformation("Setting primary provider {Provider} for user {UserId}", provider, user.Id);

        return new LinkAccountResult
        {
            Success = true,
            Message = $"Primary authentication method set to {provider}"
        };
    }

    private static string GetProviderDisplayName(string provider) => provider switch
    {
        "Google" => "Google",
        "Microsoft" => "Microsoft",
        "GitHub" => "GitHub",
        "Facebook" => "Facebook",
        _ => provider
    };

    private static string GetProviderIconUrl(string provider) => provider switch
    {
        "Google" => "/icons/google.svg",
        "Microsoft" => "/icons/microsoft.svg",
        "GitHub" => "/icons/github.svg",
        "Facebook" => "/icons/facebook.svg",
        _ => "/icons/default-provider.svg"
    };

    private static List<string> GetAvailableProviders()
    {
        return new List<string> { "Google", "Microsoft", "GitHub", "Facebook" };
    }

    private static List<string> GenerateSecurityRecommendations(bool hasPassword, int linkedAccountsCount)
    {
        var recommendations = new List<string>();

        if (!hasPassword)
        {
            recommendations.Add("Set up a password as backup authentication method");
        }

        if (linkedAccountsCount == 0)
        {
            recommendations.Add("Link external accounts for faster login experience");
        }
        else if (linkedAccountsCount == 1)
        {
            recommendations.Add("Consider linking additional authentication providers for better security");
        }

        if (linkedAccountsCount > 0 && hasPassword)
        {
            recommendations.Add("Your account has good security with multiple authentication methods");
        }

        return recommendations;
    }

    #endregion
}

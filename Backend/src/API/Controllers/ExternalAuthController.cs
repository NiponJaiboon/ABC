using Core.Constants;
using Core.Entities;
using Core.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace ABC.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExternalAuthController : ControllerBase
{
    private readonly IExternalAuthenticationService _externalAuthService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILegacyAuthenticationAuditService _auditService;
    private readonly ILogger<ExternalAuthController> _logger;

    // Constants for common messages
    private const string UserNotAuthenticatedMessage = "User not authenticated";
    private const string UserNotFoundMessage = "User not found";

    public ExternalAuthController(
        IExternalAuthenticationService externalAuthService,
        UserManager<ApplicationUser> userManager,
        ILegacyAuthenticationAuditService auditService,
        ILogger<ExternalAuthController> logger)
    {
        _externalAuthService = externalAuthService;
        _userManager = userManager;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of available external authentication providers
    /// </summary>
    /// <returns>List of supported external providers</returns>
    [HttpGet("providers")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<object>> GetProviders()
    {
        try
        {
            var providers = ExternalProviders.SupportedProviders.Select(provider => new
            {
                name = provider,
                displayName = GetProviderDisplayName(provider),
                loginUrl = Url.Action(nameof(Challenge), "ExternalAuth", new { provider }),
                iconUrl = GetProviderIconUrl(provider),
                isEnabled = true
            });

            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external providers");
            return StatusCode(500, "An error occurred while retrieving providers.");
        }
    }

    /// <summary>
    /// Initiate external authentication challenge with specified provider
    /// </summary>
    /// <param name="provider">External authentication provider (Google, Microsoft)</param>
    /// <param name="returnUrl">URL to redirect after authentication</param>
    /// <returns>Challenge result to redirect to external provider</returns>
    [HttpGet("challenge/{provider}")]
    [AllowAnonymous]
    [EnableRateLimiting("ExternalAuthPolicy")]
    public async Task<IActionResult> Challenge(string provider, [FromQuery] string returnUrl = "")
    {
        try
        {
            if (!ExternalProviders.SupportedProviders.Contains(provider))
            {
                return BadRequest(new
                {
                    message = $"Provider '{provider}' is not supported",
                    supportedProviders = ExternalProviders.SupportedProviders
                });
            }

            // Build the callback URL
            var callbackUrl = Url.Action(nameof(Callback), "ExternalAuth", new { returnUrl }, Request.Scheme) ?? "";

            var properties = await _externalAuthService.GetExternalAuthenticationPropertiesAsync(provider, callbackUrl);

            _logger.LogInformation("Initiating external authentication for provider {Provider}", provider);

            return Challenge(properties, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating external authentication for provider {Provider}", provider);
            return StatusCode(500, "An error occurred while initiating external authentication.");
        }
    }

    /// <summary>
    /// Handle external authentication callback from provider
    /// </summary>
    /// <param name="returnUrl">URL to redirect after authentication</param>
    /// <returns>Authentication result with JWT tokens or redirect</returns>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<ActionResult<ExternalAuthResult>> Callback([FromQuery] string returnUrl = "")
    {
        try
        {
            var result = await _externalAuthService.HandleExternalLoginCallbackAsync();

            if (result.Success)
            {
                // Log successful external authentication
                await _auditService.LogExternalLoginAttemptAsync(
                    result.ExternalUserInfo?.Provider ?? "Unknown",
                    result.User?.Email ?? "Unknown",
                    true,
                    result.IsNewUser,
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                _logger.LogInformation("External authentication successful for user {Email} (New User: {IsNewUser})",
                    result.User?.Email, result.IsNewUser);

                // If there's a return URL, redirect there with the tokens as query parameters
                if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
                {
                    var redirectUrl = $"{returnUrl}?token={result.AccessToken}&refresh={result.RefreshToken}&isNewUser={result.IsNewUser}";
                    return Redirect(redirectUrl);
                }

                // Return JSON response with authentication result
                return Ok(result);
            }
            else
            {
                // Log failed external authentication
                await _auditService.LogExternalLoginAttemptAsync(
                    result.ExternalUserInfo?.Provider ?? "Unknown",
                    result.ExternalUserInfo?.Email ?? "Unknown",
                    false,
                    false,
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                _logger.LogWarning("External authentication failed: {Errors}", string.Join(", ", result.Errors));

                return BadRequest(new ExternalAuthResult
                {
                    Success = false,
                    Errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling external authentication callback");
            return StatusCode(500, "An error occurred during external authentication.");
        }
    }

    /// <summary>
    /// Link an external authentication provider to the current user account
    /// </summary>
    /// <param name="model">External provider linking information</param>
    /// <returns>Result of the linking operation</returns>
    [HttpPost("link")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> LinkExternalAccount([FromBody] LinkExternalLoginModel model)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            // Initiate external login for linking
            var callbackUrl = Url.Action("LinkCallback", "ExternalAuth", null, Request.Scheme) ?? "";
            var properties = await _externalAuthService.GetExternalAuthenticationPropertiesAsync(model.Provider, callbackUrl);

            // Store user ID in authentication properties for linking
            properties.Items["userId"] = userId;

            return Challenge(properties, model.Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating external account linking for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while linking external account.");
        }
    }

    /// <summary>
    /// Handle external authentication callback for account linking
    /// </summary>
    /// <returns>Result of the linking operation</returns>
    [HttpGet("link-callback")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> LinkCallback()
    {
        try
        {
            var result = await _externalAuthService.HandleExternalLoginCallbackAsync();

            if (result.Success)
            {
                _logger.LogInformation("External account linking successful for user {Email}", result.User?.Email);

                return Ok(new AuthResult
                {
                    Success = true,
                    Errors = new List<string> { "External account linked successfully" }
                });
            }
            else
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = result.Errors
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling external account linking callback");
            return StatusCode(500, "An error occurred during external account linking.");
        }
    }

    /// <summary>
    /// Remove external authentication provider from user account
    /// </summary>
    /// <param name="model">External provider removal information</param>
    /// <returns>Result of the removal operation</returns>
    [HttpPost("unlink")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> UnlinkExternalAccount([FromBody] RemoveExternalLoginModel model)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if user has a password or other external logins before unlinking
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var externalLogins = await _userManager.GetLoginsAsync(user);

            if (!hasPassword && externalLogins.Count <= 1)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Cannot remove the last authentication method. Please set a password first." }
                });
            }

            var result = await _userManager.RemoveLoginAsync(user, model.Provider, model.ProviderKey);

            if (result.Succeeded)
            {
                _logger.LogInformation("External login {Provider} removed from user {Email}", model.Provider, user.Email);

                return Ok(new AuthResult
                {
                    Success = true,
                    Errors = new List<string> { $"{model.Provider} account unlinked successfully" }
                });
            }
            else
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking external account for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while unlinking external account.");
        }
    }

    /// <summary>
    /// Get list of external accounts linked to the current user
    /// </summary>
    /// <returns>List of linked external authentication providers</returns>
    [HttpGet("linked-accounts")]
    [Authorize]
    public async Task<ActionResult<List<object>>> GetLinkedAccounts()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var externalLogins = await _externalAuthService.GetUserExternalLoginsAsync(userId);

            var linkedAccounts = externalLogins.Select(login => new
            {
                provider = login.LoginProvider,
                providerDisplayName = GetProviderDisplayName(login.LoginProvider),
                providerKey = login.ProviderKey,
                canUnlink = true, // This would be determined by business logic
                iconUrl = GetProviderIconUrl(login.LoginProvider)
            });

            return Ok(linkedAccounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting linked accounts for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while retrieving linked accounts.");
        }
    }

    /// <summary>
    /// Get external authentication status and available actions for current user
    /// </summary>
    /// <returns>External authentication status information</returns>
    [HttpGet("status")]
    [Authorize]
    public async Task<ActionResult<object>> GetExternalAuthStatus()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            var externalLogins = await _externalAuthService.GetUserExternalLoginsAsync(userId);
            var availableProviders = ExternalProviders.SupportedProviders
                .Where(p => !externalLogins.Any(el => el.LoginProvider == p))
                .ToList();

            return Ok(new
            {
                hasPassword,
                externalLoginsCount = externalLogins.Count,
                linkedProviders = externalLogins.Select(el => el.LoginProvider),
                availableProviders,
                canUnlinkAccounts = hasPassword || externalLogins.Count > 1,
                recommendations = GetRecommendations(hasPassword, externalLogins.Count, availableProviders.Count)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external auth status for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while getting authentication status.");
        }
    }

    #region Helper Methods

    private static string GetProviderDisplayName(string provider)
    {
        return provider switch
        {
            ExternalProviders.Google => "Google",
            ExternalProviders.Microsoft => "Microsoft",
            ExternalProviders.GitHub => "GitHub",
            ExternalProviders.Facebook => "Facebook",
            _ => provider
        };
    }

    private static string GetProviderIconUrl(string provider)
    {
        return provider switch
        {
            ExternalProviders.Google => "/icons/google.svg",
            ExternalProviders.Microsoft => "/icons/microsoft.svg",
            ExternalProviders.GitHub => "/icons/github.svg",
            ExternalProviders.Facebook => "/icons/facebook.svg",
            _ => "/icons/default-provider.svg"
        };
    }

    private static List<string> GetRecommendations(bool hasPassword, int linkedProvidersCount, int availableProvidersCount)
    {
        var recommendations = new List<string>();

        if (!hasPassword && linkedProvidersCount == 1)
        {
            recommendations.Add("Consider setting up a password as backup authentication method");
        }

        if (availableProvidersCount > 0)
        {
            recommendations.Add($"You can link {availableProvidersCount} additional authentication provider(s)");
        }

        if (linkedProvidersCount == 0 && hasPassword)
        {
            recommendations.Add("Link external accounts for faster login experience");
        }

        return recommendations;
    }

    #endregion

    #region Step 9: Enhanced Account Linking Endpoints

    /// <summary>
    /// Get comprehensive account linking summary with security score
    /// </summary>
    /// <returns>Detailed account linking information and recommendations</returns>
    [HttpGet("summary")]
    [Authorize]
    public async Task<ActionResult<AccountLinkingSummary>> GetAccountSummary()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var summary = await accountLinkingService.GetAccountSummaryAsync(userId);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account summary for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while retrieving account summary.");
        }
    }

    /// <summary>
    /// Initiate enhanced account linking with conflict detection
    /// </summary>
    /// <param name="request">Enhanced linking request</param>
    /// <returns>Result of the linking initiation</returns>
    [HttpPost("link-enhanced")]
    [Authorize]
    public async Task<ActionResult<LinkAccountResult>> LinkAccountEnhanced([FromBody] LinkAccountRequest request)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var result = await accountLinkingService.InitiateLinkingAsync(userId, request);

            if (result.Success)
            {
                // If successful, initiate OAuth flow
                var callbackUrl = Url.Action("LinkCallbackEnhanced", "ExternalAuth", new { returnUrl = request.ReturnUrl }, Request.Scheme) ?? "";
                var properties = await _externalAuthService.GetExternalAuthenticationPropertiesAsync(request.Provider, callbackUrl);

                // Store user ID and request info for enhanced callback handling
                properties.Items["userId"] = userId;
                properties.Items["enhanced"] = "true";

                return Challenge(properties, request.Provider);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating enhanced account linking for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while initiating account linking.");
        }
    }

    /// <summary>
    /// Handle enhanced account linking callback with conflict resolution
    /// </summary>
    /// <param name="returnUrl">URL to redirect after linking</param>
    /// <returns>Result of the enhanced linking operation</returns>
    [HttpGet("link-callback-enhanced")]
    [Authorize]
    public async Task<ActionResult<LinkAccountResult>> LinkCallbackEnhanced([FromQuery] string returnUrl = "")
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var externalAuthResult = await _externalAuthService.HandleExternalLoginCallbackAsync();

            if (!externalAuthResult.Success || externalAuthResult.ExternalUserInfo == null)
            {
                return BadRequest(new LinkAccountResult
                {
                    Success = false,
                    Errors = externalAuthResult.Errors
                });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var result = await accountLinkingService.CompleteLinkingAsync(userId, externalAuthResult.ExternalUserInfo);

            if (result.Success)
            {
                _logger.LogInformation("Enhanced account linking successful for user {UserId} with provider {Provider}",
                    userId, externalAuthResult.ExternalUserInfo.Provider);

                // If there's a return URL, redirect there
                if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
                {
                    return Redirect($"{returnUrl}?linkingResult=success");
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling enhanced account linking callback");
            return StatusCode(500, "An error occurred during enhanced account linking.");
        }
    }

    /// <summary>
    /// Resolve account linking conflicts
    /// </summary>
    /// <param name="request">Conflict resolution request</param>
    /// <returns>Result of the conflict resolution</returns>
    [HttpPost("resolve-conflict")]
    [Authorize]
    public async Task<ActionResult<LinkAccountResult>> ResolveConflict([FromBody] ConflictResolutionRequest request)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var result = await accountLinkingService.ResolveConflictAsync(userId, request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving account linking conflict for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while resolving the conflict.");
        }
    }

    /// <summary>
    /// Get security score for the current user's account
    /// </summary>
    /// <returns>Account security score and recommendations</returns>
    [HttpGet("security-score")]
    [Authorize]
    public async Task<ActionResult<AccountSecurityScore>> GetSecurityScore()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var securityScore = await accountLinkingService.CalculateSecurityScoreAsync(userId);

            return Ok(securityScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security score for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while calculating security score.");
        }
    }

    /// <summary>
    /// Perform bulk actions on linked accounts
    /// </summary>
    /// <param name="action">Bulk action request</param>
    /// <returns>Result of the bulk action</returns>
    [HttpPost("bulk-action")]
    [Authorize]
    public async Task<ActionResult<LinkAccountResult>> BulkAccountAction([FromBody] BulkAccountAction action)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var result = await accountLinkingService.BulkAccountActionAsync(userId, action);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk account action for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while performing bulk action.");
        }
    }

    /// <summary>
    /// Check if an account can be safely unlinked
    /// </summary>
    /// <param name="provider">Provider name</param>
    /// <param name="providerKey">Provider key</param>
    /// <returns>Whether the account can be unlinked</returns>
    [HttpGet("can-unlink")]
    [Authorize]
    public async Task<ActionResult<object>> CanUnlinkAccount([FromQuery] string provider, [FromQuery] string providerKey)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = UserNotAuthenticatedMessage });
            }

            var accountLinkingService = HttpContext.RequestServices.GetRequiredService<IAccountLinkingService>();
            var canUnlink = await accountLinkingService.CanUnlinkAccountAsync(userId, provider, providerKey);

            return Ok(new
            {
                provider,
                providerKey,
                canUnlink,
                reason = canUnlink ? "Account can be safely unlinked" : "Cannot unlink - this is your only authentication method"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if account can be unlinked for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while checking unlink status.");
        }
    }

    #endregion
}

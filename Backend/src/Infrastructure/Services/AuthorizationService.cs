using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// Step 12: Authorization & Scopes - Main OAuth Authorization Service
public class OAuthAuthorizationService : IOAuthAuthorizationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOAuthClientService _clientService;
    private readonly IScopeService _scopeService;
    private readonly IConsentService _consentService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<OAuthAuthorizationService> _logger;

    public OAuthAuthorizationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IOAuthClientService clientService,
        IScopeService scopeService,
        IConsentService consentService,
        IPermissionService permissionService,
        ILogger<OAuthAuthorizationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _clientService = clientService;
        _scopeService = scopeService;
        _consentService = consentService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<bool> AuthorizeAsync(string userId, string clientId, List<string> requestedScopes)
    {
        try
        {
            // Validate user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Authorization failed: User {UserId} not found", userId);
                return false;
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Authorization failed: User {UserId} email not confirmed", userId);
                return false;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("Authorization failed: User {UserId} is locked out", userId);
                return false;
            }

            // Validate client
            var client = await _clientService.GetClientAsync(clientId);
            if (client == null)
            {
                _logger.LogWarning("Authorization failed: Client {ClientId} not found", clientId);
                return false;
            }

            // Validate scopes
            var scopeValidation = await _scopeService.ValidateScopesAsync(requestedScopes, clientId);
            if (!scopeValidation.IsValid)
            {
                _logger.LogWarning("Authorization failed: Invalid scopes {Scopes} for client {ClientId}",
                    string.Join(", ", scopeValidation.InvalidScopes), clientId);
                return false;
            }

            // Check permissions for requested scopes
            var requiredPermissions = await _scopeService.GetScopePermissionsAsync(requestedScopes);
            if (requiredPermissions.Any())
            {
                var hasAllPermissions = await _permissionService.HasAllPermissionsAsync(userId, requiredPermissions);
                if (!hasAllPermissions)
                {
                    _logger.LogWarning("Authorization failed: User {UserId} lacks required permissions for scopes {Scopes}",
                        userId, string.Join(", ", requestedScopes));
                    return false;
                }
            }

            _logger.LogInformation("Authorization successful for user {UserId}, client {ClientId}, scopes {Scopes}",
                userId, clientId, string.Join(", ", requestedScopes));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authorization for user {UserId}, client {ClientId}", userId, clientId);
            return false;
        }
    }

    public async Task<AuthorizeResult> ValidateAuthorizeRequestAsync(AuthorizeRequest request)
    {
        try
        {
            var result = new AuthorizeResult
            {
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                ResponseType = request.ResponseType,
                Scope = request.Scope,
                State = request.State,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                IsValid = false
            };

            // Validate client
            var client = await _clientService.GetClientAsync(request.ClientId);
            if (client == null)
            {
                result.Error = "invalid_client";
                result.ErrorDescription = "Client not found";
                return result;
            }

            // Validate redirect URI
            if (!await _clientService.ValidateRedirectUriAsync(request.ClientId, request.RedirectUri))
            {
                result.Error = "invalid_request";
                result.ErrorDescription = "Invalid redirect_uri";
                return result;
            }

            // Validate response type
            if (string.IsNullOrEmpty(request.ResponseType) ||
                (request.ResponseType != "code" && request.ResponseType != "token" && request.ResponseType != "id_token"))
            {
                result.Error = "unsupported_response_type";
                result.ErrorDescription = "Response type not supported";
                return result;
            }

            // Validate scopes
            var requestedScopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (!requestedScopes.Any())
            {
                result.Error = "invalid_scope";
                result.ErrorDescription = "Scope is required";
                return result;
            }

            var scopeValidation = await _scopeService.ValidateScopesAsync(requestedScopes, request.ClientId);
            if (!scopeValidation.IsValid)
            {
                result.Error = "invalid_scope";
                result.ErrorDescription = $"Invalid scopes: {string.Join(", ", scopeValidation.InvalidScopes)}";
                return result;
            }

            // Validate PKCE if required
            if (client.RequirePkce && (string.IsNullOrEmpty(request.CodeChallenge) || string.IsNullOrEmpty(request.CodeChallengeMethod)))
            {
                result.Error = "invalid_request";
                result.ErrorDescription = "PKCE is required for this client";
                return result;
            }

            result.IsValid = true;
            result.RequestedScopes = requestedScopes;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating authorize request for client {ClientId}", request.ClientId);
            return new AuthorizeResult
            {
                ClientId = request.ClientId,
                IsValid = false,
                Error = "server_error",
                ErrorDescription = "An error occurred while validating the request"
            };
        }
    }

    public async Task<Dictionary<string, object>> BuildClaimsAsync(string userId, List<string> scopes)
    {
        try
        {
            var claims = new Dictionary<string, object>();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Cannot build claims for non-existent user {UserId}", userId);
                return claims;
            }

            // Standard claims
            claims["sub"] = userId;
            claims["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Process scope-specific claims
            foreach (var scope in scopes)
            {
                switch (scope)
                {
                    case OAuthScopes.OpenId:
                        claims["sub"] = userId;
                        break;

                    case OAuthScopes.Profile:
                        if (!string.IsNullOrEmpty(user.FirstName))
                            claims["given_name"] = user.FirstName;
                        if (!string.IsNullOrEmpty(user.LastName))
                            claims["family_name"] = user.LastName;
                        if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                            claims["name"] = $"{user.FirstName} {user.LastName}".Trim();
                        if (!string.IsNullOrEmpty(user.PhoneNumber))
                            claims["phone_number"] = user.PhoneNumber;
                        claims["phone_number_verified"] = user.PhoneNumberConfirmed;
                        if (user.DateOfBirth.HasValue)
                            claims["birthdate"] = user.DateOfBirth.Value.ToString("yyyy-MM-dd");
                        break;

                    case OAuthScopes.Email:
                        if (!string.IsNullOrEmpty(user.Email))
                            claims["email"] = user.Email;
                        claims["email_verified"] = user.EmailConfirmed;
                        break;

                    case OAuthScopes.Roles:
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Any())
                            claims["roles"] = roles;

                        var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
                        if (userPermissions.Permissions.Any())
                            claims["permissions"] = userPermissions.Permissions;
                        break;

                    case OAuthScopes.PortfolioRead:
                    case OAuthScopes.PortfolioWrite:
                        claims["portfolio_access"] = scope == OAuthScopes.PortfolioWrite ? "write" : "read";
                        break;

                    case OAuthScopes.ProjectsRead:
                    case OAuthScopes.ProjectsWrite:
                        claims["projects_access"] = scope == OAuthScopes.ProjectsWrite ? "write" : "read";
                        break;

                    case OAuthScopes.SkillsRead:
                    case OAuthScopes.SkillsWrite:
                        claims["skills_access"] = scope == OAuthScopes.SkillsWrite ? "write" : "read";
                        break;
                }
            }

            // Add custom user claims
            var customClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in customClaims)
            {
                if (!claims.ContainsKey(claim.Type))
                {
                    claims[claim.Type] = claim.Value;
                }
            }

            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building claims for user {UserId}", userId);
            return new Dictionary<string, object>();
        }
    }

    public async Task<bool> ValidateTokenScopesAsync(string accessToken, List<string> requiredScopes)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken) || !requiredScopes.Any())
                return false;

            // Parse JWT token to extract scopes
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(accessToken))
            {
                _logger.LogWarning("Invalid JWT token format");
                return false;
            }

            var token = tokenHandler.ReadJwtToken(accessToken);
            var scopeClaim = token.Claims.FirstOrDefault(c => c.Type == "scope" || c.Type == "scp");

            if (scopeClaim == null)
            {
                _logger.LogWarning("No scope claim found in token");
                return false;
            }

            var tokenScopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Check if all required scopes are present in the token
            var missingScopes = requiredScopes.Except(tokenScopes).ToList();

            if (missingScopes.Any())
            {
                _logger.LogWarning("Token missing required scopes: {MissingScopes}", string.Join(", ", missingScopes));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token scopes");
            return false;
        }
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(string userId, List<string> scopes)
    {
        try
        {
            var permissions = new List<string>();

            // Get scope-based permissions
            var scopePermissions = await _scopeService.GetScopePermissionsAsync(scopes);
            permissions.AddRange(scopePermissions);

            // Get user-specific permissions
            var userPermissions = await _permissionService.GetUserPermissionsAsync(userId);
            permissions.AddRange(userPermissions.Permissions);

            // Remove duplicates and return
            return permissions.Distinct().OrderBy(p => p).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective permissions for user {UserId}", userId);
            return new List<string>();
        }
    }
}

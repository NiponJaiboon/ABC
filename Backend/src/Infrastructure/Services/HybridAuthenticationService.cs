using System.Security.Claims;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IHybridAuthenticationService
{
    Task<HybridAuthResult> LoginAsync(HybridLoginModel model, string ipAddress, string userAgent);
    Task<HybridAuthResult> RefreshTokenAsync(RefreshTokenRequest request, string userId);
    Task<bool> LogoutAsync(string userId, LogoutRequest request);
    Task<SessionStatusResponse> GetSessionStatusAsync(string userId, string? sessionId = null);
    Task<AuthResult> ValidateHybridAuthAsync(string userId, string? sessionId = null, string? accessToken = null);
}

public class HybridAuthenticationService : IHybridAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionManagementService _sessionManagementService;
    private readonly IAuthenticationAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HybridAuthenticationService> _logger;

    public HybridAuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ISessionManagementService sessionManagementService,
        IAuthenticationAuditService auditService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<HybridAuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _sessionManagementService = sessionManagementService;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<HybridAuthResult> LoginAsync(HybridLoginModel model, string ipAddress, string userAgent)
    {
        try
        {
            // Find user by email or username
            var user = await _userManager.FindByEmailAsync(model.EmailOrUsername) ??
                      await _userManager.FindByNameAsync(model.EmailOrUsername);

            if (user == null)
            {
                await _auditService.LogAuthenticationAttemptAsync(
                    model.EmailOrUsername, "password", "local", false, ipAddress, userAgent);

                return new HybridAuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (!result.Succeeded)
            {
                await _auditService.LogAuthenticationAttemptAsync(
                    user.UserName ?? "Unknown", "password", "local", false, ipAddress, userAgent);

                var errors = new List<string>();
                if (result.IsLockedOut)
                    errors.Add("Account is locked due to multiple failed attempts");
                else if (result.RequiresTwoFactor)
                    errors.Add("Two-factor authentication required");
                else
                    errors.Add("Invalid credentials");

                return new HybridAuthResult
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Create session
            var session = await _sessionManagementService.CreateSessionAsync(
                user.Id, ipAddress, userAgent, model.DeviceName, model.PreferredAuthType);

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

            // Update user with refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(HybridAuthConstants.TokenPolicy.RefreshTokenExpiryDays);
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Handle cookie authentication for hybrid/cookie mode
            if (model.PreferredAuthType == AuthType.Cookie || model.PreferredAuthType == AuthType.Hybrid)
            {
                var claims = await BuildClaimsAsync(user, session.SessionId);
                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await _signInManager.Context.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1)
                    });
            }

            await _auditService.LogAuthenticationAttemptAsync(
                user.UserName ?? "Unknown", "password", "local", true, ipAddress, userAgent);

            _logger.LogInformation("Hybrid login successful for user {UserId} with session {SessionId}",
                user.Id, session.SessionId);

            return new HybridAuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(HybridAuthConstants.TokenPolicy.AccessTokenExpiryHours),
                User = MapToUserInfo(user),
                UseCookieAuth = model.PreferredAuthType == AuthType.Cookie || model.PreferredAuthType == AuthType.Hybrid,
                UseTokenAuth = model.PreferredAuthType == AuthType.Token || model.PreferredAuthType == AuthType.Hybrid,
                SessionId = session.SessionId,
                SessionExpiresAt = session.ExpiresAt,
                Session = session
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hybrid login for user {EmailOrUsername}", model.EmailOrUsername);

            return new HybridAuthResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred during login" }
            };
        }
    }

    public async Task<HybridAuthResult> RefreshTokenAsync(RefreshTokenRequest request, string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new HybridAuthResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Validate refresh token
            if (user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid refresh token attempt for user {UserId}", userId);

                return new HybridAuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid refresh token" }
                };
            }

            // Generate new tokens
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

            // Update user with new refresh token if rotation is enabled
            if (HybridAuthConstants.TokenPolicy.RevokeTokenOnRefresh)
            {
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(HybridAuthConstants.TokenPolicy.RefreshTokenExpiryDays);
                await _userManager.UpdateAsync(user);
            }

            // Extend session if requested
            var httpContext = _httpContextAccessor.HttpContext;
            var sessionId = httpContext?.User?.FindFirst(SessionClaims.SessionId)?.Value;

            if (request.ExtendSession && !string.IsNullOrEmpty(sessionId))
            {
                await _sessionManagementService.ExtendSessionAsync(sessionId);
            }

            _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

            return new HybridAuthResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = HybridAuthConstants.TokenPolicy.RevokeTokenOnRefresh ? newRefreshToken : request.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(HybridAuthConstants.TokenPolicy.AccessTokenExpiryHours),
                User = MapToUserInfo(user),
                UseCookieAuth = true,
                UseTokenAuth = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user {UserId}", userId);

            return new HybridAuthResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred during token refresh" }
            };
        }
    }

    public async Task<bool> LogoutAsync(string userId, LogoutRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Sign out from cookie authentication
            await _signInManager.SignOutAsync();

            // Revoke sessions
            int revokedSessions;
            if (request.LogoutFromAllDevices)
            {
                revokedSessions = await _sessionManagementService.RevokeUserSessionsAsync(userId);

                // Revoke refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                await _sessionManagementService.RevokeSessionAsync(request.SessionId);
                revokedSessions = 1;
            }
            else
            {
                // Get current session from context
                var httpContext = _httpContextAccessor.HttpContext;
                var currentSessionId = httpContext?.User?.FindFirst(SessionClaims.SessionId)?.Value;

                if (!string.IsNullOrEmpty(currentSessionId))
                {
                    await _sessionManagementService.RevokeSessionAsync(currentSessionId);
                    revokedSessions = 1;
                }
                else
                {
                    revokedSessions = 0;
                }
            }

            _logger.LogInformation("User {UserId} logged out, revoked {SessionCount} sessions", userId, revokedSessions);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(string userId, string? sessionId = null)
    {
        return await _sessionManagementService.GetSessionStatusAsync(userId, sessionId);
    }

    public async Task<AuthResult> ValidateHybridAuthAsync(string userId, string? sessionId = null, string? accessToken = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found" }
                };
            }

            var isValid = true;
            var errors = new List<string>();

            // Validate session if provided
            if (!string.IsNullOrEmpty(sessionId))
            {
                var sessionValid = await _sessionManagementService.ValidateSessionAsync(sessionId);
                if (!sessionValid)
                {
                    isValid = false;
                    errors.Add("Session is invalid or expired");
                }
            }

            // Validate access token if provided
            if (!string.IsNullOrEmpty(accessToken))
            {
                var principal = await _jwtTokenService.GetPrincipalFromExpiredTokenAsync(accessToken);
                if (principal == null)
                {
                    isValid = false;
                    errors.Add("Access token is invalid");
                }
            }

            return new AuthResult
            {
                Success = isValid,
                User = isValid ? MapToUserInfo(user) : null,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating hybrid auth for user {UserId}", userId);

            return new AuthResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred during validation" }
            };
        }
    }

    private async Task<List<Claim>> BuildClaimsAsync(ApplicationUser user, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id),
            new(System.Security.Claims.ClaimTypes.Name, user.UserName ?? string.Empty),
            new(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
            new(SessionClaims.SessionId, sessionId),
            new(SessionClaims.AuthType, AuthType.Hybrid.ToString())
        };

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(System.Security.Claims.ClaimTypes.Role, role)));

        // Add user claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Add profile information
        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(System.Security.Claims.ClaimTypes.GivenName, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(System.Security.Claims.ClaimTypes.Surname, user.LastName));

        return claims;
    }

    private static UserInfo MapToUserInfo(ApplicationUser user)
    {
        return new UserInfo
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty,
            Bio = user.Bio ?? string.Empty,
            Website = user.Website ?? string.Empty,
            Location = user.Location ?? string.Empty,
            CreatedAt = user.CreatedAt
        };
    }
}

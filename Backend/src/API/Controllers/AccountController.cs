using System.ComponentModel.DataAnnotations;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace ABC.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly IExternalAuthenticationService _externalAuthService;
    private readonly ILogger<AccountController> _logger;

    // Step 11: Hybrid Authentication Services
    private readonly IHybridAuthenticationService _hybridAuthService;
    private readonly ISessionManagementService _sessionManagementService;

    // Step 14: Audit Services
    private readonly CompositeAuditService _auditService;
    private readonly IFailedLoginTrackingService _failedLoginService;
    private readonly ISecurityAuditService _securityAuditService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IPasswordPolicyService passwordPolicyService,
        IExternalAuthenticationService externalAuthService,
        ILogger<AccountController> logger,
        IHybridAuthenticationService hybridAuthService,
        ISessionManagementService sessionManagementService,
        CompositeAuditService auditService,
        IFailedLoginTrackingService failedLoginService,
        ISecurityAuditService securityAuditService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _passwordPolicyService = passwordPolicyService;
        _externalAuthService = externalAuthService;
        _logger = logger;
        _hybridAuthService = hybridAuthService;
        _sessionManagementService = sessionManagementService;
        _auditService = auditService;
        _failedLoginService = failedLoginService;
        _securityAuditService = securityAuditService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="model">Registration details</param>
    /// <returns>Authentication result with JWT tokens</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Enhanced Password validation using custom policy
            var passwordValidation = await _passwordPolicyService.ValidatePasswordAsync(
                model.Password, model.UserName, model.Email);

            if (!passwordValidation.Succeeded)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = passwordValidation.Errors.Select(e => e.Description).ToList()
                });
            }

            // Check if user with email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "A user with this email address already exists." }
                });
            }

            // Check if user with username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUsername != null)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "A user with this username already exists." }
                });
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = false, // Will require email confirmation
                CreatedAt = DateTime.UtcNow,
                LastLogin = null
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Step 14: Log registration failure
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                await _auditService.LogRegistrationAsync(
                    string.Empty, model.UserName, model.Email, false, errors, HttpContext);

                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, Roles.User);

            _logger.LogInformation("User {Email} registered successfully", model.Email);

            // Step 14: Log registration success
            await _auditService.LogRegistrationAsync(
                user.Id, user.UserName, user.Email, true, null, HttpContext);

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Based on JWT settings
                User = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = userRoles.ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", model.Email);
            return StatusCode(500, new AuthResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred during registration. Please try again." }
            });
        }
    }

    /// <summary>
    /// Login with email/username and password
    /// </summary>
    /// <param name="model">Login credentials</param>
    /// <returns>Authentication result with JWT tokens</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            // Find user by email or username
            var user = await _userManager.FindByEmailAsync(model.EmailOrUsername) ??
                      await _userManager.FindByNameAsync(model.EmailOrUsername);

            if (user == null)
            {
                // Step 14: Log failed login for unknown user
                await _auditService.LogFailedLoginAsync(
                    null, model.EmailOrUsername, null, AuthenticationMethods.Local,
                    "User not found", HttpContext);

                return Unauthorized(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email/username or password." }
                });
            }

            // Check if account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                // Step 14: Log locked account access attempt
                await _auditService.LogFailedLoginAsync(
                    user.Id, user.UserName, user.Email, AuthenticationMethods.Local,
                    "Account locked", HttpContext);

                return Unauthorized(new AuthResult
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        $"Account is locked. Try again after {lockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss")}."
                    }
                });
            }

            // Attempt sign in
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                var errors = new List<string>();
                string failureReason;

                if (result.IsLockedOut)
                {
                    failureReason = "Account locked due to multiple failed attempts";
                    errors.Add("Account has been locked due to multiple failed attempts.");
                }
                else if (result.RequiresTwoFactor)
                {
                    failureReason = "Two-factor authentication required";
                    errors.Add("Two-factor authentication is required.");
                }
                else if (result.IsNotAllowed)
                {
                    failureReason = "Email not confirmed";
                    errors.Add("Sign in is not allowed. Please confirm your email address.");
                }
                else
                {
                    failureReason = "Invalid credentials";
                    errors.Add("Invalid email/username or password.");
                }

                // Step 14: Log failed login attempt
                await _auditService.LogFailedLoginAsync(
                    user?.Id, user?.UserName ?? model.EmailOrUsername, user?.Email,
                    AuthenticationMethods.Local, failureReason, HttpContext);

                return Unauthorized(new AuthResult
                {
                    Success = false,
                    Errors = errors
                });
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Step 14: Log successful login
            await _auditService.LogSuccessfulLoginAsync(
                user.Id, user.UserName, user.Email, AuthenticationMethods.Local, null, HttpContext);

            return Ok(new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Based on JWT settings
                User = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = userRoles.ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {EmailOrUsername}", model.EmailOrUsername);
            return StatusCode(500, new AuthResult
            {
                Success = false,
                Errors = new List<string> { "An error occurred during login. Please try again." }
            });
        }
    }

    #region External Authentication

    /// <summary>
    /// Initiate external authentication with the specified provider
    /// </summary>
    /// <param name="provider">External authentication provider (Google, Microsoft)</param>
    /// <param name="returnUrl">URL to redirect after authentication</param>
    /// <returns>Challenge result to redirect to external provider</returns>
    [HttpGet("external-login/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = "")
    {
        try
        {
            if (!ExternalProviders.SupportedProviders.Contains(provider))
            {
                return BadRequest(new { message = $"Provider '{provider}' is not supported" });
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl }) ?? "";
            var properties = await _externalAuthService.GetExternalAuthenticationPropertiesAsync(provider, redirectUrl);

            return Challenge(properties, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating external login for provider {Provider}", provider);
            return StatusCode(500, "An error occurred while initiating external authentication.");
        }
    }

    /// <summary>
    /// Handle external authentication callback
    /// </summary>
    /// <param name="returnUrl">URL to redirect after authentication</param>
    /// <returns>Authentication result with JWT tokens</returns>
    [HttpGet("external-login-callback")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> ExternalLoginCallback(string returnUrl = "")
    {
        try
        {
            var result = await _externalAuthService.HandleExternalLoginCallbackAsync();

            if (result.Success)
            {
                _logger.LogInformation("External login successful for user {Email}", result.User?.Email);

                return Ok(new AuthResult
                {
                    Success = true,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = new UserInfo
                    {
                        Id = result.User!.Id,
                        UserName = result.User.UserName!,
                        Email = result.User.Email!,
                        FirstName = result.User.FirstName,
                        LastName = result.User.LastName,
                        EmailConfirmed = result.User.EmailConfirmed,
                        ProfilePictureUrl = result.User.ProfilePictureUrl,
                        CreatedAt = result.User.CreatedAt
                    },
                    IsNewUser = result.IsNewUser,
                    IsExternalLogin = true,
                    ExternalProvider = result.ExternalUserInfo?.Provider ?? ""
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
            _logger.LogError(ex, "Error handling external login callback");
            return StatusCode(500, "An error occurred during external authentication.");
        }
    }

    /// <summary>
    /// Get user's external login providers
    /// </summary>
    /// <returns>List of linked external providers</returns>
    [HttpGet("external-logins")]
    [Authorize]
    public async Task<ActionResult<List<Core.Models.ExternalProviderInfo>>> GetExternalLogins()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var externalLogins = await _externalAuthService.GetUserExternalLoginsAsync(userId);
            return Ok(externalLogins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user external logins");
            return StatusCode(500, "An error occurred while retrieving external logins.");
        }
    }

    #endregion

    #region Step 11: Hybrid Authentication Endpoints

    /// <summary>
    /// Hybrid login supporting both cookie and JWT authentication
    /// </summary>
    [HttpPost("hybrid-login")]
    [AllowAnonymous]
    public async Task<ActionResult<HybridAuthResult>> HybridLogin([FromBody] HybridLoginModel model)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _hybridAuthService.LoginAsync(model, ipAddress, userAgent);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hybrid authentication");
            return StatusCode(500, "An error occurred during authentication");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<HybridAuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Get user ID from the refresh token claim or decode from access token
            var userId = await GetUserIdFromRefreshTokenAsync(request.RefreshToken);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            var result = await _hybridAuthService.RefreshTokenAsync(request, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "An error occurred while refreshing the token");
        }
    }

    /// <summary>
    /// Hybrid logout supporting both cookie and JWT authentication
    /// </summary>
    [HttpPost("hybrid-logout")]
    [Authorize]
    public async Task<ActionResult> HybridLogout([FromBody] LogoutRequest? request = null)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var success = await _hybridAuthService.LogoutAsync(userId, request ?? new LogoutRequest());

            if (success)
            {
                // Step 14: Log logout
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var sessionId = User.FindFirst(SessionClaims.SessionId)?.Value;
                    await _auditService.LogLogoutAsync(userId, user.UserName!, sessionId, HttpContext);
                }

                return Ok(new { message = "Logged out successfully" });
            }

            return BadRequest(new { message = "Failed to logout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hybrid logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }

    /// <summary>
    /// Get current session information
    /// </summary>
    [HttpGet("session/status")]
    [Authorize]
    public async Task<ActionResult<SessionStatusResponse>> GetSessionStatus()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;
            var sessionId = User.FindFirst(SessionClaims.SessionId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _sessionManagementService.GetSessionStatusAsync(userId, sessionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status");
            return StatusCode(500, "An error occurred while getting session status");
        }
    }

    /// <summary>
    /// Get all user sessions
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<SessionInfo>>> GetUserSessions()
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sessions = await _sessionManagementService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions");
            return StatusCode(500, "An error occurred while getting sessions");
        }
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<ActionResult> RevokeSession(string sessionId)
    {
        try
        {
            var userId = User.FindFirst(SystemClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify the session belongs to the current user
            var session = await _sessionManagementService.GetSessionAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound("Session not found");
            }

            var success = await _sessionManagementService.RevokeSessionAsync(sessionId);

            if (success)
            {
                return Ok(new { message = "Session revoked successfully" });
            }

            return BadRequest("Failed to revoke session");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while revoking the session");
        }
    }

    /// <summary>
    /// Extend current session expiration
    /// </summary>
    [HttpPost("session/extend")]
    [Authorize]
    public async Task<ActionResult> ExtendSession([FromBody] int? extensionMinutes = null)
    {
        try
        {
            var sessionId = User.FindFirst(SessionClaims.SessionId)?.Value;

            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("No active session found");
            }

            TimeSpan? extension = extensionMinutes.HasValue ? TimeSpan.FromMinutes(extensionMinutes.Value) : null;
            var success = await _sessionManagementService.ExtendSessionAsync(sessionId, extension);

            if (success)
            {
                return Ok(new { message = "Session extended successfully" });
            }

            return BadRequest("Failed to extend session");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            return StatusCode(500, "An error occurred while extending the session");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<string?> GetUserIdFromRefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find user by refresh token
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                         u.RefreshTokenExpiryTime > DateTime.UtcNow);

            return user?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID from refresh token");
            return null;
        }
    }

    #endregion
}

public class ValidatePasswordRequest
{
    public string Password { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
}

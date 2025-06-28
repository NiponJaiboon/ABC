using System.Security.Claims;
using Core.Constants;
using Core.Entities;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IExternalAuthenticationService
{
    Task<AuthenticationProperties> GetExternalAuthenticationPropertiesAsync(string provider, string redirectUrl);
    Task<ExternalAuthResult> HandleExternalLoginCallbackAsync();
    Task<List<Core.Models.ExternalProviderInfo>> GetUserExternalLoginsAsync(string userId);
}

public class ExternalAuthenticationService : IExternalAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<ExternalAuthenticationService> _logger;

    public ExternalAuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<ExternalAuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public Task<AuthenticationProperties> GetExternalAuthenticationPropertiesAsync(string provider, string redirectUrl)
    {
        if (!ExternalProviders.SupportedProviders.Contains(provider))
        {
            throw new ArgumentException($"Provider '{provider}' is not supported", nameof(provider));
        }

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        properties.Items["scheme"] = provider;
        properties.Items["returnUrl"] = redirectUrl;

        return Task.FromResult(properties);
    }

    public async Task<ExternalAuthResult> HandleExternalLoginCallbackAsync()
    {
        var result = new ExternalAuthResult();

        try
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                result.Errors.Add("Error loading external login information");
                return result;
            }

            // Try to sign in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                // User exists and signed in successfully
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    result.Success = true;
                    result.AccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
                    result.RefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
                    result.User = user;
                    result.IsNewUser = false;

                    _logger.LogInformation("External login successful for user {Email} via {Provider}",
                        user.Email, info.LoginProvider);
                }
            }
            else
            {
                // Handle new user registration or account linking
                await HandleNewExternalUserAsync(info, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling external login callback");
            result.Errors.Add("An error occurred during external authentication");
        }

        return result;
    }

    public async Task<List<Core.Models.ExternalProviderInfo>> GetUserExternalLoginsAsync(string userId)
    {
        var result = new List<Core.Models.ExternalProviderInfo>();

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return result;

            var logins = await _userManager.GetLoginsAsync(user);

            result = logins.Select(login => new Core.Models.ExternalProviderInfo
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external logins for user {UserId}", userId);
        }

        return result;
    }

    private async Task HandleNewExternalUserAsync(Microsoft.AspNetCore.Identity.ExternalLoginInfo info, ExternalAuthResult result)
    {
        // Get user info from claims
        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email) ?? string.Empty;

        if (string.IsNullOrEmpty(email))
        {
            result.Errors.Add("Email not provided by external provider");
            return;
        }

        // Check if user with this email already exists
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            // Link external login to existing account
            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
            if (addLoginResult.Succeeded)
            {
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                result.Success = true;
                result.AccessToken = await _jwtTokenService.GenerateAccessTokenAsync(existingUser);
                result.RefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
                result.User = existingUser;
                result.IsNewUser = false;
            }
            else
            {
                result.Errors.AddRange(addLoginResult.Errors.Select(e => e.Description));
            }
        }
        else
        {
            // Create new user
            var newUser = await CreateUserFromExternalInfoAsync(info);
            var createResult = await _userManager.CreateAsync(newUser);

            if (createResult.Succeeded)
            {
                var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
                if (addLoginResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, Roles.User);
                    await _signInManager.SignInAsync(newUser, isPersistent: false);

                    result.Success = true;
                    result.AccessToken = await _jwtTokenService.GenerateAccessTokenAsync(newUser);
                    result.RefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
                    result.User = newUser;
                    result.IsNewUser = true;
                }
                else
                {
                    result.Errors.AddRange(addLoginResult.Errors.Select(e => e.Description));
                    await _userManager.DeleteAsync(newUser);
                }
            }
            else
            {
                result.Errors.AddRange(createResult.Errors.Select(e => e.Description));
            }
        }
    }

    private async Task<ApplicationUser> CreateUserFromExternalInfoAsync(Microsoft.AspNetCore.Identity.ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email) ?? string.Empty;
        var firstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName) ?? string.Empty;
        var lastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname) ?? string.Empty;

        // Generate unique username from email
        var username = await GenerateUniqueUsernameAsync(email);

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true, // External providers usually verify emails
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        return user;
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email)
    {
        var baseUsername = email.Split('@')[0].ToLower();
        var username = baseUsername;
        var counter = 1;

        while (await _userManager.FindByNameAsync(username) != null)
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
    }
}

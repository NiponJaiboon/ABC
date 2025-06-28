using System.ComponentModel.DataAnnotations;

namespace Core.Models;

public class RegisterModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginModel
{
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class ChangePasswordModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public UserInfo User { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsNewUser { get; set; }
    public bool IsExternalLogin { get; set; }
    public string ExternalProvider { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<ExternalLoginInfo> ExternalLogins { get; set; } = new();
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ExternalLoginInfo
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
}

public class ExternalAuthRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}

public class RefreshTokenModel
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

// Step 11: Hybrid Authentication Flow Models
public class HybridAuthResult : AuthResult
{
    public bool UseCookieAuth { get; set; }
    public bool UseTokenAuth { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime? SessionExpiresAt { get; set; }
    public SessionInfo Session { get; set; }
}

public class SessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastAccessed { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public AuthType AuthType { get; set; }
}

public class SessionStatusResponse
{
    public bool IsActive { get; set; }
    public SessionInfo Session { get; set; }
    public List<SessionInfo> AllSessions { get; set; } = new();
    public int ActiveSessionCount { get; set; }
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    public bool ExtendSession { get; set; } = true;
}

public class LogoutRequest
{
    public bool LogoutFromAllDevices { get; set; } = false;
    public string SessionId { get; set; } = string.Empty;
}

public class HybridLoginModel : LoginModel
{
    public AuthType PreferredAuthType { get; set; } = AuthType.Hybrid;
    public bool CreateSession { get; set; } = true;
    public string DeviceName { get; set; } = string.Empty;
}

public enum AuthType
{
    Cookie,
    Token,
    Hybrid
}

using Core.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IPasswordPolicyService
{
    Task<IdentityResult> ValidatePasswordAsync(string password, string? userName = null, string? email = null);
    IEnumerable<string> GetPasswordRequirements();
    bool IsPasswordCompromised(string password);
}

public class PasswordPolicyService : IPasswordPolicyService
{
    private readonly ILogger<PasswordPolicyService> _logger;

    // Common weak passwords (you can expand this or load from a file)
    private readonly HashSet<string> _commonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "123456", "123456789", "qwerty", "abc123", "password123",
        "admin", "letmein", "welcome", "monkey", "1234567890", "Password1",
        "123123", "password1", "admin123", "root", "toor", "pass", "test",
        "guest", "user", "demo", "sample", "temp", "default", "changeme"
    };

    public PasswordPolicyService(ILogger<PasswordPolicyService> logger)
    {
        _logger = logger;
    }

    public Task<IdentityResult> ValidatePasswordAsync(string password, string? userName = null, string? email = null)
    {
        var errors = new List<IdentityError>();

        // Length validation
        if (password.Length < SecurityConstants.PasswordPolicy.MinimumLength)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = string.Format(SecurityConstants.PasswordMessages.TooShort,
                    SecurityConstants.PasswordPolicy.MinimumLength)
            });
        }

        if (password.Length > SecurityConstants.PasswordPolicy.MaximumLength)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooLong",
                Description = string.Format(SecurityConstants.PasswordMessages.TooLong,
                    SecurityConstants.PasswordPolicy.MaximumLength)
            });
        }

        // Character requirements
        if (SecurityConstants.PasswordPolicy.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = SecurityConstants.PasswordMessages.RequireDigit
            });
        }

        if (SecurityConstants.PasswordPolicy.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresLower",
                Description = SecurityConstants.PasswordMessages.RequireLowercase
            });
        }

        if (SecurityConstants.PasswordPolicy.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUpper",
                Description = SecurityConstants.PasswordMessages.RequireUppercase
            });
        }

        if (SecurityConstants.PasswordPolicy.RequireNonAlphanumeric &&
            password.All(char.IsLetterOrDigit))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = SecurityConstants.PasswordMessages.RequireNonAlphanumeric
            });
        }

        // Unique characters requirement
        var uniqueChars = password.Distinct().Count();
        if (uniqueChars < SecurityConstants.PasswordPolicy.RequiredUniqueChars)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUniqueChars",
                Description = string.Format(SecurityConstants.PasswordMessages.RequireUniqueChars,
                    SecurityConstants.PasswordPolicy.RequiredUniqueChars)
            });
        }

        // Common password check
        if (IsPasswordCompromised(password))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooCommon",
                Description = SecurityConstants.PasswordMessages.CommonPassword
            });
        }

        // User info check
        if (!string.IsNullOrEmpty(userName) &&
            password.Contains(userName, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordContainsUsername",
                Description = SecurityConstants.PasswordMessages.ContainsUserInfo
            });
        }

        if (!string.IsNullOrEmpty(email))
        {
            var emailPrefix = email.Split('@')[0];
            if (password.Contains(emailPrefix, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsEmail",
                    Description = SecurityConstants.PasswordMessages.ContainsUserInfo
                });
            }
        }

        return Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }

    public IEnumerable<string> GetPasswordRequirements()
    {
        var requirements = new List<string>();

        requirements.Add($"At least {SecurityConstants.PasswordPolicy.MinimumLength} characters long");
        requirements.Add($"No more than {SecurityConstants.PasswordPolicy.MaximumLength} characters");

        if (SecurityConstants.PasswordPolicy.RequireDigit)
            requirements.Add("At least one digit (0-9)");

        if (SecurityConstants.PasswordPolicy.RequireLowercase)
            requirements.Add("At least one lowercase letter (a-z)");

        if (SecurityConstants.PasswordPolicy.RequireUppercase)
            requirements.Add("At least one uppercase letter (A-Z)");

        if (SecurityConstants.PasswordPolicy.RequireNonAlphanumeric)
            requirements.Add("At least one special character (!@#$%^&*)");

        requirements.Add($"At least {SecurityConstants.PasswordPolicy.RequiredUniqueChars} unique characters");
        requirements.Add("Cannot be a common password");
        requirements.Add("Cannot contain parts of your username or email");

        return requirements;
    }

    public bool IsPasswordCompromised(string password)
    {
        // Check against common passwords
        if (_commonPasswords.Contains(password))
        {
            _logger.LogWarning("Common password attempt detected");
            return true;
        }

        // Check for simple patterns
        if (IsSimplePattern(password))
        {
            _logger.LogWarning("Simple pattern password detected");
            return true;
        }

        return false;
    }

    private bool IsSimplePattern(string password)
    {
        // Check for keyboard patterns
        var keyboardPatterns = new[]
        {
            "qwerty", "asdf", "zxcv", "1234", "abcd", "qwertyuiop",
            "asdfghjkl", "zxcvbnm", "12345", "123456", "1234567890"
        };

        foreach (var pattern in keyboardPatterns)
        {
            if (password.ToLower().Contains(pattern))
                return true;
        }

        // Check for repetitive patterns
        if (IsRepetitive(password))
            return true;

        return false;
    }

    private bool IsRepetitive(string password)
    {
        // Check for repeated characters (more than 3 in a row)
        for (int i = 0; i < password.Length - 3; i++)
        {
            if (password[i] == password[i + 1] &&
                password[i + 1] == password[i + 2] &&
                password[i + 2] == password[i + 3])
            {
                return true;
            }
        }

        // Check for simple sequences
        for (int i = 0; i < password.Length - 3; i++)
        {
            if (Math.Abs(password[i] - password[i + 1]) == 1 &&
                Math.Abs(password[i + 1] - password[i + 2]) == 1 &&
                Math.Abs(password[i + 2] - password[i + 3]) == 1)
            {
                return true;
            }
        }

        return false;
    }
}

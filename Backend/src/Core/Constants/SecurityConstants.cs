namespace Core.Constants;

public static class SecurityConstants
{
    // Password Policy
    public static class PasswordPolicy
    {
        public const int MinimumLength = 8;
        public const int MaximumLength = 128;
        public const bool RequireDigit = true;
        public const bool RequireLowercase = true;
        public const bool RequireUppercase = true;
        public const bool RequireNonAlphanumeric = true;
        public const int RequiredUniqueChars = 4;
    }

    // Account Lockout Policy
    public static class LockoutPolicy
    {
        public const bool DefaultLockoutEnabled = true;
        public const int MaxFailedAccessAttempts = 5;
        public const int LockoutTimeSpanMinutes = 15;
        public const bool AllowedForNewUsers = true;
    }

    // Session & Token Policy
    public static class SessionPolicy
    {
        public const int AccessTokenLifetimeMinutes = 60;
        public const int RefreshTokenLifetimeDays = 7;
        public const int CookieTimeoutMinutes = 60;
        public const bool RequireConfirmedEmail = false; // For development
        public const bool RequireConfirmedPhoneNumber = false;
    }

    // Rate Limiting
    public static class RateLimiting
    {
        public const int LoginAttemptsPerMinute = 5;
        public const int RegistrationAttemptsPerHour = 3;
        public const int PasswordResetAttemptsPerHour = 3;
        public const int TokenRefreshAttemptsPerMinute = 10;
    }

    // Security Headers
    public static class SecurityHeaders
    {
        public const string ContentSecurityPolicy =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';";

        public const string ReferrerPolicy = "strict-origin-when-cross-origin";
        public const string PermissionsPolicy =
            "camera=(), microphone=(), geolocation=(), " +
            "payment=(), usb=(), magnetometer=(), gyroscope=()";
    }

    // Password Validation Messages
    public static class PasswordMessages
    {
        public const string TooShort = "Password must be at least {0} characters long.";
        public const string TooLong = "Password cannot exceed {0} characters.";
        public const string RequireDigit = "Password must contain at least one digit (0-9).";
        public const string RequireLowercase = "Password must contain at least one lowercase letter (a-z).";
        public const string RequireUppercase = "Password must contain at least one uppercase letter (A-Z).";
        public const string RequireNonAlphanumeric = "Password must contain at least one special character (!@#$%^&*).";
        public const string RequireUniqueChars = "Password must contain at least {0} unique characters.";
        public const string CommonPassword = "This password is too common. Please choose a more secure password.";
        public const string ContainsUserInfo = "Password cannot contain parts of your username or email.";
    }
}

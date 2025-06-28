namespace Core.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Moderator = "Moderator";
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string UserOrAdmin = "UserOrAdmin";
    public const string ModeratorOrAdmin = "ModeratorOrAdmin";
    public const string EmailVerified = "EmailVerified";
    public const string CanCreatePortfolio = "CanCreatePortfolio";
    public const string CanModerateContent = "CanModerateContent";
}

public static class ClaimTypes
{
    public const string EmailVerified = "email_verified";
    public const string UserId = "user_id";
    public const string ProfilePicture = "picture";
}

public static class ExternalProviders
{
    public const string Google = "Google";
    public const string Microsoft = "Microsoft";
    public const string GitHub = "GitHub";
    public const string Facebook = "Facebook";

    public static readonly string[] SupportedProviders = { Google, Microsoft };
}

public static class ExternalClaims
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string GivenName = "given_name";
    public const string FamilyName = "family_name";
    public const string Picture = "picture";
    public const string Locale = "locale";
}

public static class SessionClaims
{
    public const string SessionId = "session_id";
    public const string DeviceName = "device_name";
    public const string AuthType = "auth_type";
    public const string IpAddress = "ip_address";
}

// Step 12: Authorization & Scopes Constants
public static class OAuthScopes
{
    public const string OpenId = "openid";
    public const string Profile = "profile";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string Roles = "roles";
    public const string Portfolio = "portfolio";
    public const string Projects = "projects";
    public const string Skills = "skills";
    public const string Admin = "admin";

    // Fine-grained access scopes
    public const string PortfolioRead = "portfolio:read";
    public const string PortfolioWrite = "portfolio:write";
    public const string PortfolioManage = "portfolio:manage";

    public const string ProjectRead = "project:read";
    public const string ProjectWrite = "project:write";
    public const string ProjectManage = "project:manage";

    public const string ProjectsRead = "projects:read";
    public const string ProjectsWrite = "projects:write";

    public const string SkillsRead = "skills:read";
    public const string SkillsWrite = "skills:write";

    // Composite scopes
    public const string FullAccess = "full_access";
    public const string ReadOnly = "read_only";

    public static readonly string[] StandardScopes = { OpenId, Profile, Email };
    public static readonly string[] ApplicationScopes = { Portfolio, Projects, Skills };
    public static readonly string[] AllScopes = { OpenId, Profile, Email, Roles, Portfolio, Projects, Skills, Admin, FullAccess, ReadOnly };
}

public static class Permissions
{
    // Portfolio permissions
    public const string PortfolioRead = "portfolio:read";
    public const string PortfolioWrite = "portfolio:write";
    public const string PortfolioDelete = "portfolio:delete";
    public const string PortfolioShare = "portfolio:share";

    // Project permissions
    public const string ProjectRead = "project:read";
    public const string ProjectWrite = "project:write";
    public const string ProjectDelete = "project:delete";
    public const string ProjectPublish = "project:publish";

    // Skill permissions
    public const string SkillRead = "skill:read";
    public const string SkillWrite = "skill:write";
    public const string SkillDelete = "skill:delete";

    // Admin permissions
    public const string UserManagement = "user:management";
    public const string SystemAdmin = "system:admin";
    public const string AuditLogs = "audit:logs";

    // User permissions
    public const string ProfileRead = "profile:read";
    public const string ProfileWrite = "profile:write";
    public const string AccountManagement = "account:management";
}

public static class ClientTypes
{
    public const string Web = "web";
    public const string Mobile = "mobile";
    public const string Desktop = "desktop";
    public const string Api = "api";
    public const string Service = "service";
}

public static class GrantTypes
{
    public const string AuthorizationCode = "authorization_code";
    public const string ClientCredentials = "client_credentials";
    public const string RefreshToken = "refresh_token";
    public const string Implicit = "implicit";
    public const string DeviceCode = "device_code";
}

public static class HybridAuthConstants
{
    public static class SessionPolicy
    {
        public const int MaxSessionsPerUser = 5;
        public const int SessionTimeoutMinutes = 60;
        public const int SlidingExpirationMinutes = 30;
        public const bool EnableConcurrentSessions = true;
    }

    public static class TokenPolicy
    {
        public const int AccessTokenExpiryHours = 1;
        public const int RefreshTokenExpiryDays = 7;
        public const int MaxRefreshTokensPerUser = 3;
        public const bool EnableTokenRefresh = true;
        public const bool RevokeTokenOnRefresh = true;
    }
}

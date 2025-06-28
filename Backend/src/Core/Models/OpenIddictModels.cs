using System.ComponentModel.DataAnnotations;

namespace Core.Models;

// OpenIddict Authorization Models for Step 10
public class OpenIddictModels
{
    // Authorization Request Models
    public class AuthorizeRequest
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        public string RedirectUri { get; set; } = string.Empty;

        [Required]
        public string ResponseType { get; set; } = string.Empty;

        public string Scope { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        // PKCE parameters
        public string CodeChallenge { get; set; } = string.Empty;
        public string CodeChallengeMethod { get; set; } = string.Empty;

        // OpenID Connect parameters
        public string Nonce { get; set; } = string.Empty;
        public string ResponseMode { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }

    // Token Request Models
    public class TokenRequest
    {
        [Required]
        public string GrantType { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // Authorization Code Grant
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;

        // Refresh Token Grant
        public string RefreshToken { get; set; } = string.Empty;

        // Password Grant (if enabled)
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Scopes
        public string Scope { get; set; } = string.Empty;
    }

    // Client Registration Models
    public class ClientRegistrationRequest
    {
        [Required]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        public string ClientType { get; set; } = string.Empty; // confidential, public

        [Required]
        public List<string> RedirectUris { get; set; } = new();

        public List<string> PostLogoutRedirectUris { get; set; } = new();

        [Required]
        public List<string> GrantTypes { get; set; } = new();

        public List<string> Scopes { get; set; } = new();

        public string Description { get; set; } = string.Empty;

        public string LogoUri { get; set; } = string.Empty;

        public string TermsOfServiceUri { get; set; } = string.Empty;

        public string PolicyUri { get; set; } = string.Empty;

        // PKCE settings
        public bool RequirePkce { get; set; } = true;

        // Token settings
        public int AccessTokenLifetime { get; set; } = 3600; // 1 hour
        public int RefreshTokenLifetime { get; set; } = 7200; // 2 hours
    }

    // Client Information Response
    public class ClientInfo
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientType { get; set; } = string.Empty;
        public List<string> RedirectUris { get; set; } = new();
        public List<string> PostLogoutRedirectUris { get; set; } = new();
        public List<string> GrantTypes { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
        public bool RequirePkce { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Scope Definition Models
    public class ScopeDefinition
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> Claims { get; set; } = new();

        public bool IsRequired { get; set; } = false;

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public string IconUrl { get; set; } = string.Empty;
    }

    // User Consent Models
    public class ConsentRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientDescription { get; set; } = string.Empty;
        public string ClientLogoUri { get; set; } = string.Empty;
        public List<ScopeConsentInfo> RequestedScopes { get; set; } = new();
        public string ReturnUrl { get; set; } = string.Empty;
        public bool RememberConsent { get; set; } = false;
    }

    public class ScopeConsentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public bool IsGranted { get; set; } = false;
    }

    public class ConsentResponse
    {
        public List<string> GrantedScopes { get; set; } = new();
        public bool RememberConsent { get; set; } = false;
        public string Action { get; set; } = string.Empty; // allow, deny
    }

    // Authorization Result Models
    public class AuthorizationResult
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public string ErrorDescription { get; set; } = string.Empty;
    }

    // Token Response Models
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty; // For OpenID Connect
    }

    // Discovery Document Models
    public class DiscoveryDocument
    {
        public string Issuer { get; set; } = string.Empty;
        public string AuthorizationEndpoint { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string UserInfoEndpoint { get; set; } = string.Empty;
        public string JwksUri { get; set; } = string.Empty;
        public string EndSessionEndpoint { get; set; } = string.Empty;
        public List<string> ScopesSupported { get; set; } = new();
        public List<string> ResponseTypesSupported { get; set; } = new();
        public List<string> GrantTypesSupported { get; set; } = new();
        public List<string> SubjectTypesSupported { get; set; } = new();
        public List<string> IdTokenSigningAlgValuesSupported { get; set; } = new();
        public List<string> CodeChallengeMethodsSupported { get; set; } = new();
        public bool ClaimsParameterSupported { get; set; } = true;
        public bool RequestParameterSupported { get; set; } = true;
    }
}

// Custom Scope Constants
public static class OpenIddictScopes
{
    // Standard OpenID Connect scopes
    public const string OpenId = "openid";
    public const string Profile = "profile";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string Address = "address";

    // Custom application scopes
    public const string PortfolioRead = "portfolio:read";
    public const string PortfolioWrite = "portfolio:write";
    public const string ProjectsRead = "projects:read";
    public const string ProjectsWrite = "projects:write";
    public const string SkillsRead = "skills:read";
    public const string SkillsWrite = "skills:write";
    public const string AdminAccess = "admin:access";
    public const string UserManagement = "user:management";

    // Offline access
    public const string OfflineAccess = "offline_access";

    public static IReadOnlyDictionary<string, OpenIddictModels.ScopeDefinition> AllScopes { get; } = new Dictionary<string, OpenIddictModels.ScopeDefinition>
    {
        [OpenId] = new OpenIddictModels.ScopeDefinition
        {
            Name = OpenId,
            DisplayName = "OpenID Connect",
            Description = "Access to your identity information",
            Claims = new List<string> { "sub" },
            IsRequired = true
        },
        [Profile] = new OpenIddictModels.ScopeDefinition
        {
            Name = Profile,
            DisplayName = "Profile Information",
            Description = "Access to your profile information (name, username, etc.)",
            Claims = new List<string> { "name", "preferred_username", "given_name", "family_name", "picture" }
        },
        [Email] = new OpenIddictModels.ScopeDefinition
        {
            Name = Email,
            DisplayName = "Email Address",
            Description = "Access to your email address",
            Claims = new List<string> { "email", "email_verified" }
        },
        [PortfolioRead] = new OpenIddictModels.ScopeDefinition
        {
            Name = PortfolioRead,
            DisplayName = "Read Portfolio",
            Description = "View your portfolio information",
            Claims = new List<string> { "portfolio:read" }
        },
        [PortfolioWrite] = new OpenIddictModels.ScopeDefinition
        {
            Name = PortfolioWrite,
            DisplayName = "Manage Portfolio",
            Description = "Create and modify your portfolio information",
            Claims = new List<string> { "portfolio:read", "portfolio:write" }
        },
        [ProjectsRead] = new OpenIddictModels.ScopeDefinition
        {
            Name = ProjectsRead,
            DisplayName = "Read Projects",
            Description = "View your projects",
            Claims = new List<string> { "projects:read" }
        },
        [ProjectsWrite] = new OpenIddictModels.ScopeDefinition
        {
            Name = ProjectsWrite,
            DisplayName = "Manage Projects",
            Description = "Create and modify your projects",
            Claims = new List<string> { "projects:read", "projects:write" }
        },
        [OfflineAccess] = new OpenIddictModels.ScopeDefinition
        {
            Name = OfflineAccess,
            DisplayName = "Offline Access",
            Description = "Access your data when you're offline",
            Claims = new List<string>()
        }
    };
}

// Grant Type Constants
public static class OpenIddictGrantTypes
{
    public const string AuthorizationCode = "authorization_code";
    public const string RefreshToken = "refresh_token";
    public const string Password = "password";
    public const string ClientCredentials = "client_credentials";
    public const string Implicit = "implicit";
    public const string DeviceCode = "urn:ietf:params:oauth:grant-type:device_code";
}

// Response Type Constants
public static class OpenIddictResponseTypes
{
    public const string Code = "code";
    public const string Token = "token";
    public const string IdToken = "id_token";
    public const string CodeToken = "code token";
    public const string CodeIdToken = "code id_token";
    public const string TokenIdToken = "token id_token";
    public const string CodeTokenIdToken = "code token id_token";
}

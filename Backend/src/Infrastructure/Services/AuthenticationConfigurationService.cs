using System.Text;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public static class AuthenticationConfigurationService
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Configuration
        var jwtSecret = configuration["JwtSettings:Secret"] ??
            throw new InvalidOperationException("JWT Secret not found in configuration");
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true
            };

            // Handle JWT events
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    // Log authentication failures
                    Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Log successful token validation
                    Console.WriteLine("JWT Token validated successfully");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    // Allow token from query string for SignalR connections
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        // Add External Authentication Providers
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? string.Empty;
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
            options.CallbackPath = "/signin-google";

            // Request additional scopes
            options.Scope.Add("profile");
            options.Scope.Add("email");

            // Save tokens for later use
            options.SaveTokens = true;

            // Map claims
            options.ClaimActions.MapJsonKey("picture", "picture");
            options.ClaimActions.MapJsonKey("locale", "locale");
        })
        .AddMicrosoftAccount(options =>
        {
            options.ClientId = configuration["Authentication:Microsoft:ClientId"] ?? string.Empty;
            options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"] ?? string.Empty;
            options.CallbackPath = "/signin-microsoft";

            // Request additional scopes
            options.Scope.Add("https://graph.microsoft.com/user.read");

            // Save tokens for later use
            options.SaveTokens = true;

            // Map claims
            options.ClaimActions.MapJsonKey("picture", "picture");
        });

        return services;
    }

    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services)
    {
        // Configure Identity options with enhanced security
        services.Configure<IdentityOptions>(options =>
        {
            // Enhanced Password settings
            options.Password.RequireDigit = SecurityConstants.PasswordPolicy.RequireDigit;
            options.Password.RequireLowercase = SecurityConstants.PasswordPolicy.RequireLowercase;
            options.Password.RequireNonAlphanumeric = SecurityConstants.PasswordPolicy.RequireNonAlphanumeric;
            options.Password.RequireUppercase = SecurityConstants.PasswordPolicy.RequireUppercase;
            options.Password.RequiredLength = SecurityConstants.PasswordPolicy.MinimumLength;
            options.Password.RequiredUniqueChars = SecurityConstants.PasswordPolicy.RequiredUniqueChars;

            // Enhanced Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(SecurityConstants.LockoutPolicy.LockoutTimeSpanMinutes);
            options.Lockout.MaxFailedAccessAttempts = SecurityConstants.LockoutPolicy.MaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = SecurityConstants.LockoutPolicy.AllowedForNewUsers;

            // User settings
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign in settings with enhanced security
            options.SignIn.RequireConfirmedEmail = SecurityConstants.SessionPolicy.RequireConfirmedEmail;
            options.SignIn.RequireConfirmedPhoneNumber = SecurityConstants.SessionPolicy.RequireConfirmedPhoneNumber;
        });

        return services;
    }

    public static IServiceCollection AddCookieConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure application cookies for hybrid approach
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use Always in production
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Name = "ABC.Portfolio.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;

            // Custom paths for API
            options.LoginPath = "/api/account/login";
            options.LogoutPath = "/api/account/logout";
            options.AccessDeniedPath = "/api/account/access-denied";

            // Return JSON responses for API calls
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Default policy requires authenticated user
            options.FallbackPolicy = options.DefaultPolicy;            // Role-based policies
            options.AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.UserOrAdmin, policy =>
                policy.RequireRole(Roles.User, Roles.Admin));

            options.AddPolicy(Policies.ModeratorOrAdmin, policy =>
                policy.RequireRole(Roles.Moderator, Roles.Admin));

            // Claim-based policies
            options.AddPolicy(Policies.EmailVerified, policy =>
                policy.RequireClaim(ClaimTypes.EmailVerified, "true"));

            // Custom policies for portfolio management
            options.AddPolicy(Policies.CanCreatePortfolio, policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole(Roles.User, Roles.Admin));

            options.AddPolicy(Policies.CanModerateContent, policy =>
                policy.RequireRole(Roles.Moderator, Roles.Admin));
        });

        return services;
    }

    public static IServiceCollection AddSecurityHeaders(
        this IServiceCollection services)
    {
        // HSTS will be configured in the API project
        // This method is kept for consistency
        return services;
    }

    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services)
    {
        // Register password policy service
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();

        // Register security header service
        services.AddScoped<ISecurityHeaderService, SecurityHeaderService>();

        // Register external authentication service
        services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();

        // Register authentication audit service
        services.AddScoped<IAuthenticationAuditService, AuthenticationAuditService>();

        // Register account linking service (Step 9)
        services.AddScoped<IAccountLinkingService, AccountLinkingService>();

        // Step 11: Register session management and hybrid authentication services
        services.AddScoped<ISessionManagementService, SessionManagementService>();
        services.AddScoped<IHybridAuthenticationService, HybridAuthenticationService>();

        // Step 12: Register authorization and scope services
        services.AddScoped<IOAuthClientService, OAuthClientService>();
        services.AddScoped<IScopeService, ScopeService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IOAuthAuthorizationService, OAuthAuthorizationService>();

        return services;
    }

    public static IServiceCollection AddAllAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services
            .AddJwtAuthentication(configuration)
            .AddIdentityConfiguration()
            .AddCookieConfiguration(configuration)
            .AddAuthorizationPolicies()
            .AddSecurityHeaders()
            .AddSecurityServices()
            .AddOpenIddictServer(configuration)
            .AddOpenIddictServices();
    }
}

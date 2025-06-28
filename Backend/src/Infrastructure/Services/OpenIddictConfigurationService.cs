using System.Security.Claims;
using Core.Entities;
using Core.Models;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infrastructure.Services;

/// <summary>
/// OpenIddict server configuration service for Step 10
/// </summary>
public static class OpenIddictConfigurationService
{
    public static IServiceCollection AddOpenIddictServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure OpenIddict server
        services.AddOpenIddict()

            // Register the OpenIddict core components
            .AddCore(options =>
            {
                // Configure the entities framework integration
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })

            // Register the OpenIddict server components
            .AddServer(options =>
            {
                // Set the authorization and token endpoints
                options.SetAuthorizationEndpointUris("/connect/authorize")
                       .SetTokenEndpointUris("/connect/token")
                       .SetUserinfoEndpointUris("/connect/userinfo")
                       .SetLogoutEndpointUris("/connect/logout");

                // Mark the authorization code and refresh token flows as allowed
                options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow();

                // Enable PKCE for enhanced security
                options.RequireProofKeyForCodeExchange();

                // Register scopes (permissions)
                options.RegisterScopes(
                    Scopes.OpenId,
                    Scopes.Profile,
                    Scopes.Email,
                    OpenIddictScopes.PortfolioRead,
                    OpenIddictScopes.PortfolioWrite,
                    OpenIddictScopes.ProjectsRead,
                    OpenIddictScopes.ProjectsWrite,
                    OpenIddictScopes.OfflineAccess);

                // Configure signing and encryption credentials
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    // Use ephemeral keys for development (temporary solution for macOS certificate issues)
                    // Note: In production, you should use proper certificates
                    options.AddEphemeralEncryptionKey()
                           .AddEphemeralSigningKey();
                }

                // Configure token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1))
                       .SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                // Register the ASP.NET Core host and configure options
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .DisableTransportSecurityRequirement(); // Allow HTTP in development

                // Configure issuer
                options.SetIssuer(configuration["OpenIddict:Issuer"] ?? "https://localhost:7080");
            })

            // Register the OpenIddict validation components
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance
                options.UseLocalServer();

                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });

        return services;
    }

    public static IServiceCollection AddOpenIddictServices(this IServiceCollection services)
    {
        // OpenIddict automatically registers its own managers and services
        // We don't need to implement custom services for basic functionality
        return services;
    }
}

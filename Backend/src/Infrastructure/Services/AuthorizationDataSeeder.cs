using Core.Constants;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// Step 12: Data Seeder for Authorization System
public class AuthorizationDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthorizationDataSeeder> _logger;

    public AuthorizationDataSeeder(ApplicationDbContext context, ILogger<AuthorizationDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDefaultDataAsync()
    {
        try
        {
            await SeedScopeDefinitionsAsync();
            await SeedDefaultClientAsync();
            SeedDefaultPermissions();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Authorization default data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding authorization default data");
            throw;
        }
    }

    private async Task SeedScopeDefinitionsAsync()
    {
        var scopes = new[]
        {
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.OpenId,
                DisplayName = "OpenID",
                Description = "OpenID Connect identifier",
                IsRequired = true,
                IsDefault = true,
                Permissions = "[]", // JSON array as string
                Category = "Identity",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.Profile,
                DisplayName = "Profile",
                Description = "Access to user profile information",
                IsRequired = false,
                IsDefault = true,
                Permissions = $"[\"{Permissions.ProfileRead}\"]",
                Category = "Profile",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.Email,
                DisplayName = "Email",
                Description = "Access to user email address",
                IsRequired = false,
                IsDefault = true,
                Permissions = $"[\"{Permissions.ProfileRead}\"]",
                Category = "Profile",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.Roles,
                DisplayName = "Roles & Permissions",
                Description = "Access to user roles and permissions",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.ProfileRead}\"]",
                Category = "Security",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.PortfolioRead,
                DisplayName = "Portfolio Read",
                Description = "Read access to portfolio data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.PortfolioRead}\"]",
                Category = "Portfolio",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.PortfolioWrite,
                DisplayName = "Portfolio Write",
                Description = "Write access to portfolio data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.PortfolioRead}\",\"{Permissions.PortfolioWrite}\"]",
                Category = "Portfolio",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.ProjectsRead,
                DisplayName = "Projects Read",
                Description = "Read access to project data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.ProjectRead}\"]",
                Category = "Projects",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.ProjectsWrite,
                DisplayName = "Projects Write",
                Description = "Write access to project data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.ProjectRead}\",\"{Permissions.ProjectWrite}\"]",
                Category = "Projects",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.SkillsRead,
                DisplayName = "Skills Read",
                Description = "Read access to skill data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.SkillRead}\"]",
                Category = "Skills",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.SkillsWrite,
                DisplayName = "Skills Write",
                Description = "Write access to skill data",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.SkillRead}\",\"{Permissions.SkillWrite}\"]",
                Category = "Skills",
                CreatedAt = DateTime.UtcNow
            },
            new ScopeDefinitionEntity
            {
                Name = OAuthScopes.Admin,
                DisplayName = "Administrator",
                Description = "Full administrative access",
                IsRequired = false,
                IsDefault = false,
                Permissions = $"[\"{Permissions.UserManagement}\",\"{Permissions.SystemAdmin}\",\"{Permissions.AuditLogs}\"]",
                Category = "Administration",
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var scope in scopes)
        {
            var existingScope = await _context.ScopeDefinitions
                .FirstOrDefaultAsync(s => s.Name == scope.Name);

            if (existingScope == null)
            {
                _context.ScopeDefinitions.Add(scope);
                _logger.LogInformation("Added scope: {ScopeName}", scope.Name);
            }
        }
    }

    private async Task SeedDefaultClientAsync()
    {
        var defaultClientId = "abc-portfolio-spa";
        var existingClient = await _context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == defaultClientId);

        if (existingClient == null)
        {
            var defaultClient = new OAuthClient
            {
                ClientId = defaultClientId,
                ClientName = "ABC Portfolio SPA",
                ClientType = ClientTypes.Web,
                Description = "Default Single Page Application client for ABC Portfolio",
                RedirectUris = "[\"http://localhost:3000/auth/callback\",\"https://localhost:3000/auth/callback\"]",
                PostLogoutRedirectUris = "[\"http://localhost:3000\",\"https://localhost:3000\"]",
                Scopes = $"[\"{OAuthScopes.OpenId}\",\"{OAuthScopes.Profile}\",\"{OAuthScopes.Email}\",\"{OAuthScopes.PortfolioRead}\",\"{OAuthScopes.PortfolioWrite}\",\"{OAuthScopes.ProjectsRead}\",\"{OAuthScopes.ProjectsWrite}\",\"{OAuthScopes.SkillsRead}\",\"{OAuthScopes.SkillsWrite}\"]",
                GrantTypes = $"[\"{GrantTypes.AuthorizationCode}\",\"{GrantTypes.RefreshToken}\"]",
                RequirePkce = true,
                RequireClientSecret = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null // Explicitly set to null for system clients
            };

            _context.OAuthClients.Add(defaultClient);
            _logger.LogInformation("Added default client: {ClientId}", defaultClientId);
        }
    }

    private void SeedDefaultPermissions()
    {
        // This would typically involve setting up default role-permission mappings
        // For now, we'll just log that permissions are available
        var permissions = new[]
        {
            Permissions.PortfolioRead,
            Permissions.PortfolioWrite,
            Permissions.PortfolioDelete,
            Permissions.PortfolioShare,
            Permissions.ProjectRead,
            Permissions.ProjectWrite,
            Permissions.ProjectDelete,
            Permissions.ProjectPublish,
            Permissions.SkillRead,
            Permissions.SkillWrite,
            Permissions.SkillDelete,
            Permissions.UserManagement,
            Permissions.SystemAdmin,
            Permissions.AuditLogs,
            Permissions.ProfileRead,
            Permissions.ProfileWrite,
            Permissions.AccountManagement
        };

        _logger.LogInformation("Available permissions: {Permissions}", string.Join(", ", permissions));
    }
}

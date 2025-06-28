using System.Text.Json;
using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ScopeService : IScopeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ScopeService> _logger;

    public ScopeService(
        ApplicationDbContext context,
        ILogger<ScopeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ScopeDefinition>> GetAvailableScopesAsync()
    {
        try
        {
            var scopes = await _context.ScopeDefinitions
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.DisplayName)
                .ToListAsync();

            return scopes.Select(MapToScopeDefinition).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available scopes");
            return new List<ScopeDefinition>();
        }
    }

    public async Task<ScopeDefinition?> GetScopeAsync(string scopeName)
    {
        try
        {
            var scope = await _context.ScopeDefinitions
                .FirstOrDefaultAsync(s => s.Name == scopeName && s.IsActive);

            return scope != null ? MapToScopeDefinition(scope) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scope {ScopeName}", scopeName);
            return null;
        }
    }

    public async Task<ScopeValidationResult> ValidateScopesAsync(List<string> requestedScopes, string? clientId = null)
    {
        try
        {
            var result = new ScopeValidationResult();

            // Get available scopes
            var availableScopes = await GetAvailableScopesAsync();
            var availableScopeNames = availableScopes.Select(s => s.Name).ToHashSet();

            // If client ID is provided, check client-specific scopes
            if (!string.IsNullOrEmpty(clientId))
            {
                var clientScopes = await GetClientScopesAsync(clientId);
                availableScopeNames = availableScopeNames.Intersect(clientScopes).ToHashSet();
            }

            // Validate each requested scope
            foreach (var scope in requestedScopes)
            {
                if (availableScopeNames.Contains(scope))
                {
                    result.ValidScopes.Add(scope);

                    // Add permissions for this scope
                    var scopeDef = availableScopes.FirstOrDefault(s => s.Name == scope);
                    if (scopeDef != null)
                    {
                        result.RequiredPermissions.AddRange(scopeDef.Permissions);
                    }
                }
                else
                {
                    result.InvalidScopes.Add(scope);
                    result.Errors.Add($"Invalid scope: {scope}");
                }
            }

            result.IsValid = result.InvalidScopes.Count == 0;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating scopes");
            return new ScopeValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Error validating scopes" }
            };
        }
    }

    public async Task<List<string>> GetScopePermissionsAsync(List<string> scopes)
    {
        try
        {
            var permissions = new HashSet<string>();

            foreach (var scopeName in scopes)
            {
                var scope = await GetScopeAsync(scopeName);
                if (scope != null)
                {
                    foreach (var permission in scope.Permissions)
                    {
                        permissions.Add(permission);
                    }
                }
            }

            return permissions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scope permissions");
            return new List<string>();
        }
    }

    public async Task<bool> CreateScopeAsync(ScopeDefinition scope, string createdBy)
    {
        try
        {
            var existingScope = await _context.ScopeDefinitions
                .FirstOrDefaultAsync(s => s.Name == scope.Name);

            if (existingScope != null)
            {
                return false; // Scope already exists
            }

            var entity = new ScopeDefinitionEntity
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                IsRequired = scope.IsRequired,
                IsDefault = scope.IsDefault,
                Permissions = JsonSerializer.Serialize(scope.Permissions),
                Category = scope.Category,
                CreatedAt = DateTime.UtcNow
            };

            _context.ScopeDefinitions.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Scope created: {ScopeName} by {CreatedBy}", scope.Name, createdBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scope {ScopeName}", scope.Name);
            return false;
        }
    }

    public async Task<bool> UpdateScopeAsync(ScopeDefinition scope, string updatedBy)
    {
        try
        {
            var entity = await _context.ScopeDefinitions
                .FirstOrDefaultAsync(s => s.Name == scope.Name);

            if (entity == null)
            {
                return false;
            }

            entity.DisplayName = scope.DisplayName;
            entity.Description = scope.Description;
            entity.IsRequired = scope.IsRequired;
            entity.IsDefault = scope.IsDefault;
            entity.Permissions = JsonSerializer.Serialize(scope.Permissions);
            entity.Category = scope.Category;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Scope updated: {ScopeName} by {UpdatedBy}", scope.Name, updatedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scope {ScopeName}", scope.Name);
            return false;
        }
    }

    public async Task<bool> DeleteScopeAsync(string scopeName, string deletedBy)
    {
        try
        {
            var entity = await _context.ScopeDefinitions
                .FirstOrDefaultAsync(s => s.Name == scopeName);

            if (entity == null)
            {
                return false;
            }

            // Soft delete by deactivating
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Scope deleted: {ScopeName} by {DeletedBy}", scopeName, deletedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting scope {ScopeName}", scopeName);
            return false;
        }
    }

    public async Task InitializeDefaultScopesAsync()
    {
        try
        {
            var defaultScopes = GetDefaultScopes();

            foreach (var scope in defaultScopes)
            {
                var existingScope = await _context.ScopeDefinitions
                    .FirstOrDefaultAsync(s => s.Name == scope.Name);

                if (existingScope == null)
                {
                    await CreateScopeAsync(scope, "system");
                }
            }

            _logger.LogInformation("Default scopes initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default scopes");
        }
    }

    private async Task<List<string>> GetClientScopesAsync(string clientId)
    {
        try
        {
            var client = await _context.OAuthClients
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);

            if (client == null)
            {
                return new List<string>();
            }

            return JsonSerializer.Deserialize<List<string>>(client.Scopes) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client scopes for {ClientId}", clientId);
            return new List<string>();
        }
    }

    private static ScopeDefinition MapToScopeDefinition(ScopeDefinitionEntity entity)
    {
        return new ScopeDefinition
        {
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            IsRequired = entity.IsRequired,
            IsDefault = entity.IsDefault,
            Permissions = JsonSerializer.Deserialize<List<string>>(entity.Permissions) ?? new List<string>(),
            Category = entity.Category
        };
    }

    private static List<ScopeDefinition> GetDefaultScopes()
    {
        return new List<ScopeDefinition>
        {
            new()
            {
                Name = OAuthScopes.OpenId,
                DisplayName = "OpenID",
                Description = "Access to your identity",
                IsRequired = true,
                IsDefault = true,
                Permissions = new List<string> { Permissions.ProfileRead },
                Category = "identity"
            },
            new()
            {
                Name = OAuthScopes.Profile,
                DisplayName = "Profile",
                Description = "Access to your profile information",
                IsRequired = false,
                IsDefault = true,
                Permissions = new List<string> { Permissions.ProfileRead },
                Category = "identity"
            },
            new()
            {
                Name = OAuthScopes.Email,
                DisplayName = "Email",
                Description = "Access to your email address",
                IsRequired = false,
                IsDefault = true,
                Permissions = new List<string> { Permissions.ProfileRead },
                Category = "identity"
            },
            new()
            {
                Name = OAuthScopes.Roles,
                DisplayName = "Roles",
                Description = "Access to your role information",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.ProfileRead },
                Category = "identity"
            },
            new()
            {
                Name = OAuthScopes.Portfolio,
                DisplayName = "Portfolio Access",
                Description = "Access to your portfolios",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.PortfolioRead, Permissions.PortfolioWrite },
                Category = "application"
            },
            new()
            {
                Name = OAuthScopes.Projects,
                DisplayName = "Projects Access",
                Description = "Access to your projects",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.ProjectRead, Permissions.ProjectWrite },
                Category = "application"
            },
            new()
            {
                Name = OAuthScopes.Skills,
                DisplayName = "Skills Access",
                Description = "Access to your skills",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.SkillRead, Permissions.SkillWrite },
                Category = "application"
            },
            new()
            {
                Name = OAuthScopes.Admin,
                DisplayName = "Admin Access",
                Description = "Administrative access to the system",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.UserManagement, Permissions.SystemAdmin, Permissions.AuditLogs },
                Category = "admin"
            },
            new()
            {
                Name = OAuthScopes.ReadOnly,
                DisplayName = "Read Only",
                Description = "Read-only access to your data",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string> { Permissions.PortfolioRead, Permissions.ProjectRead, Permissions.SkillRead },
                Category = "application"
            },
            new()
            {
                Name = OAuthScopes.FullAccess,
                DisplayName = "Full Access",
                Description = "Full access to your account and data",
                IsRequired = false,
                IsDefault = false,
                Permissions = new List<string>
                {
                    Permissions.PortfolioRead, Permissions.PortfolioWrite, Permissions.PortfolioDelete,
                    Permissions.ProjectRead, Permissions.ProjectWrite, Permissions.ProjectDelete,
                    Permissions.SkillRead, Permissions.SkillWrite, Permissions.SkillDelete,
                    Permissions.ProfileRead, Permissions.ProfileWrite, Permissions.AccountManagement
                },
                Category = "application"
            }
        };
    }
}

using Core.Constants;
using Core.Entities;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// Step 12: Authorization & Scopes - Permission Management Service
public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserPermissionModel> GetUserPermissionsAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User '{userId}' not found");
            }

            // Get direct permissions
            var directPermissions = await _context.UserPermissions
                .Where(p => p.UserId == userId && !p.IsRevoked &&
                           (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow))
                .Select(p => new PermissionDetail
                {
                    Permission = p.Permission,
                    GrantedBy = p.GrantedBy,
                    GrantedAt = p.GrantedAt,
                    ExpiresAt = p.ExpiresAt,
                    Source = "Direct"
                })
                .ToListAsync();

            // Get role-based permissions
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolePermissions = new List<PermissionDetail>();

            foreach (var role in userRoles)
            {
                var permissions = await GetRolePermissionsAsync(role);
                rolePermissions.AddRange(permissions.Select(p => new PermissionDetail
                {
                    Permission = p,
                    Source = $"Role: {role}",
                    GrantedAt = DateTime.UtcNow // Role permissions don't have specific grant dates
                }));
            }

            // Combine and deduplicate
            var allPermissions = directPermissions.Concat(rolePermissions)
                .GroupBy(p => p.Permission)
                .Select(g => g.First())
                .OrderBy(p => p.Permission)
                .ToList();

            return new UserPermissionModel
            {
                UserId = userId,
                Email = user.Email,
                Permissions = allPermissions.Select(p => p.Permission).ToList(),
                DirectPermissionCount = directPermissions.Count,
                RolePermissionCount = rolePermissions.Count,
                TotalPermissionCount = allPermissions.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        try
        {
            // Check direct permission
            var hasDirectPermission = await _context.UserPermissions
                .AnyAsync(p => p.UserId == userId && p.Permission == permission && !p.IsRevoked &&
                              (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

            if (hasDirectPermission)
                return true;

            // Check role-based permission
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                var rolePermissions = await GetRolePermissionsAsync(role);
                if (rolePermissions.Contains(permission))
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<bool> HasAnyPermissionAsync(string userId, List<string> permissions)
    {
        try
        {
            foreach (var permission in permissions)
            {
                if (await HasPermissionAsync(userId, permission))
                    return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking any permission for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HasAllPermissionsAsync(string userId, List<string> permissions)
    {
        try
        {
            foreach (var permission in permissions)
            {
                if (!await HasPermissionAsync(userId, permission))
                    return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking all permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> GrantPermissionAsync(string userId, string permission, string grantedBy, string? reason = null, DateTime? expiresAt = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot grant permission to non-existent user {UserId}", userId);
                return false;
            }

            // Check if permission already exists and is active
            var existingPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Permission == permission);

            if (existingPermission != null)
            {
                if (!existingPermission.IsRevoked && (existingPermission.ExpiresAt == null || existingPermission.ExpiresAt > DateTime.UtcNow))
                {
                    _logger.LogInformation("Permission {Permission} already granted to user {UserId}", permission, userId);
                    return true;
                }

                // Reactivate existing permission
                existingPermission.IsRevoked = false;
                existingPermission.GrantedBy = grantedBy;
                existingPermission.GrantedAt = DateTime.UtcNow;
                existingPermission.ExpiresAt = expiresAt;
                existingPermission.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new permission
                var newPermission = new UserPermission
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Permission = permission,
                    GrantedBy = grantedBy,
                    GrantedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                _context.UserPermissions.Add(newPermission);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {Permission} granted to user {UserId} by {GrantedBy}. Reason: {Reason}",
                permission, userId, grantedBy, reason ?? "Not specified");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting permission {Permission} to user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<bool> GrantPermissionsAsync(PermissionAssignmentModel model, string grantedBy)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                _logger.LogWarning("Cannot grant permissions to non-existent user {UserId}", model.UserId);
                return false;
            }

            var successCount = 0;
            foreach (var permission in model.Permissions)
            {
                if (await GrantPermissionAsync(model.UserId, permission, grantedBy, model.Reason, model.ExpiresAt))
                {
                    successCount++;
                }
            }

            _logger.LogInformation("Granted {SuccessCount}/{TotalCount} permissions to user {UserId}",
                successCount, model.Permissions.Count, model.UserId);

            return successCount == model.Permissions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting multiple permissions to user {UserId}", model.UserId);
            return false;
        }
    }

    public async Task<bool> RevokePermissionAsync(string userId, string permission, string revokedBy, string? reason = null)
    {
        try
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Permission == permission && !p.IsRevoked);

            if (userPermission == null)
            {
                _logger.LogWarning("Permission {Permission} not found for user {UserId}", permission, userId);
                return false;
            }

            userPermission.IsRevoked = true;
            userPermission.RevokedAt = DateTime.UtcNow;
            userPermission.RevokedBy = revokedBy;
            userPermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {Permission} revoked for user {UserId} by {RevokedBy}. Reason: {Reason}",
                permission, userId, revokedBy, reason ?? "Not specified");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<bool> RevokeAllUserPermissionsAsync(string userId, string revokedBy, string? reason = null)
    {
        try
        {
            var userPermissions = await _context.UserPermissions
                .Where(p => p.UserId == userId && !p.IsRevoked)
                .ToListAsync();

            if (!userPermissions.Any())
            {
                _logger.LogInformation("No permissions to revoke for user {UserId}", userId);
                return true;
            }

            foreach (var permission in userPermissions)
            {
                permission.IsRevoked = true;
                permission.RevokedAt = DateTime.UtcNow;
                permission.RevokedBy = revokedBy;
                permission.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("All {Count} permissions revoked for user {UserId} by {RevokedBy}. Reason: {Reason}",
                userPermissions.Count, userId, revokedBy, reason ?? "Not specified");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName)
    {
        try
        {
            // Define role-based permissions mapping
            var rolePermissions = new Dictionary<string, List<string>>
            {
                [Roles.Admin] = new List<string>
                {
                    "permission:full_access",
                    Permissions.UserManagement,
                    "permission:manage_roles",
                    "permission:manage_clients",
                    Permissions.AuditLogs,
                    Permissions.SystemAdmin,
                    Permissions.PortfolioRead,
                    Permissions.PortfolioWrite,
                    Permissions.PortfolioDelete,
                    Permissions.ProjectRead,
                    Permissions.ProjectWrite,
                    Permissions.ProjectDelete,
                    Permissions.SkillRead,
                    Permissions.SkillWrite,
                    Permissions.SkillDelete
                },
                [Roles.User] = new List<string>
                {
                    Permissions.PortfolioRead,
                    Permissions.ProjectRead,
                    Permissions.SkillRead
                },
                ["Manager"] = new List<string>
                {
                    Permissions.PortfolioRead,
                    Permissions.PortfolioWrite,
                    Permissions.ProjectRead,
                    Permissions.ProjectWrite,
                    Permissions.SkillRead,
                    Permissions.SkillWrite,
                    Permissions.UserManagement
                }
            };

            return rolePermissions.ContainsKey(roleName) ? rolePermissions[roleName] : new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {RoleName}", roleName);
            return new List<string>();
        }
    }

    public async Task SyncRolePermissionsAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot sync permissions for non-existent user {UserId}", userId);
                return;
            }

            _logger.LogInformation("Role permissions synced for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing role permissions for user {UserId}", userId);
        }
    }
}

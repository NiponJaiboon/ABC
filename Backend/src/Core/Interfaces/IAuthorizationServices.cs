using Core.Entities;
using Core.Models;

namespace Core.Interfaces;

// Step 12: Authorization & Scopes Service Interfaces

public interface IOAuthClientService
{
    Task<ClientRegistrationResult> RegisterClientAsync(ClientRegistrationModel model, string createdBy);
    Task<ClientInfo?> GetClientAsync(string clientId);
    Task<ClientListResponse> GetClientsAsync(int pageNumber = 1, int pageSize = 10, string? createdBy = null);
    Task<bool> UpdateClientAsync(ClientUpdateModel model, string updatedBy);
    Task<bool> DeleteClientAsync(string clientId, string deletedBy);
    Task<bool> ValidateClientAsync(string clientId, string? clientSecret = null);
    Task<bool> ValidateRedirectUriAsync(string clientId, string redirectUri);
    Task<List<string>> GetClientScopesAsync(string clientId);
}

public interface IScopeService
{
    Task<List<ScopeDefinition>> GetAvailableScopesAsync();
    Task<ScopeDefinition?> GetScopeAsync(string scopeName);
    Task<ScopeValidationResult> ValidateScopesAsync(List<string> requestedScopes, string? clientId = null);
    Task<List<string>> GetScopePermissionsAsync(List<string> scopes);
    Task<bool> CreateScopeAsync(ScopeDefinition scope, string createdBy);
    Task<bool> UpdateScopeAsync(ScopeDefinition scope, string updatedBy);
    Task<bool> DeleteScopeAsync(string scopeName, string deletedBy);
    Task InitializeDefaultScopesAsync();
}

public interface IConsentService
{
    Task<ConsentModel> GetConsentModelAsync(string userId, string clientId, List<string> requestedScopes);
    Task<ConsentResponse> ProcessConsentAsync(string userId, string clientId, List<string> grantedScopes, bool rememberConsent);
    Task<bool> HasValidConsentAsync(string userId, string clientId, List<string> requestedScopes);
    Task<List<UserConsent>> GetUserConsentsAsync(string userId);
    Task<bool> RevokeConsentAsync(string userId, string clientId, string? reason = null);
    Task<bool> RevokeAllUserConsentsAsync(string userId);
}

public interface IPermissionService
{
    Task<UserPermissionModel> GetUserPermissionsAsync(string userId);
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<bool> HasAnyPermissionAsync(string userId, List<string> permissions);
    Task<bool> HasAllPermissionsAsync(string userId, List<string> permissions);
    Task<bool> GrantPermissionAsync(string userId, string permission, string grantedBy, string? reason = null, DateTime? expiresAt = null);
    Task<bool> GrantPermissionsAsync(PermissionAssignmentModel model, string grantedBy);
    Task<bool> RevokePermissionAsync(string userId, string permission, string revokedBy, string? reason = null);
    Task<bool> RevokeAllUserPermissionsAsync(string userId, string revokedBy, string? reason = null);
    Task<List<string>> GetRolePermissionsAsync(string roleName);
    Task SyncRolePermissionsAsync(string userId);
}

public interface IOAuthAuthorizationService
{
    Task<bool> AuthorizeAsync(string userId, string clientId, List<string> requestedScopes);
    Task<AuthorizeResult> ValidateAuthorizeRequestAsync(AuthorizeRequest request);
    Task<Dictionary<string, object>> BuildClaimsAsync(string userId, List<string> scopes);
    Task<bool> ValidateTokenScopesAsync(string accessToken, List<string> requiredScopes);
    Task<List<string>> GetEffectivePermissionsAsync(string userId, List<string> scopes);
}
